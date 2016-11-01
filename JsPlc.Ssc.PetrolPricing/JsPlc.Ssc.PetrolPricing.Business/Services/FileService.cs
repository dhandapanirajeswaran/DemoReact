using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class FileService : IFileService
	{
		private readonly IPriceService _priceService;

		private readonly IPetrolPricingRepository _db;

		private readonly IDataFileReader _dataFileReader;

		private readonly IAppSettings _appSettings;

		public FileService(IPetrolPricingRepository db,
			IPriceService priceService,
			IAppSettings appSettings,
			IDataFileReader dataFileReader)
		{
			_db = db;
			_priceService = priceService;
			_dataFileReader = dataFileReader;
			_appSettings = appSettings;
		}

		public FileUpload NewUpload(FileUpload fileUpload)
		{
			FileUpload newUpload = _db.NewUpload(fileUpload);

			List<FileUpload> newUploadList = new List<FileUpload> {
				newUpload
			};

			FileUpload processedFile;

			// Use a fire and forget approach
			switch (newUpload.UploadTypeId)
			{
				case 1:
					processedFile = ProcessDailyPrice(newUploadList);

					if (processedFile == null)
						throw new FileUploadException("Upload failed. Contact support team.");

					runRecalc(processedFile);

					break;
				case 2:
					processedFile = ProcessQuarterlyFileNew(newUploadList);

					if (processedFile == null)
						throw new FileUploadException("Upload failed. Contact support team.");

					runRecalc(processedFile);

					break;

                case 3:
                    processedFile = ProcessLatestPriceFileNew(newUploadList);

                    if (processedFile == null)
                        throw new FileUploadException("Upload failed. Contact support team.");


                    break;
				default:
					throw new InvalidOperationException(string.Format("Unsupported file type: {0}. Contact support team." + newUpload.UploadTypeId));
			}
			return newUpload;
		}

		public bool ExistsUpload(string storedFileName)
		{
			return _db.ExistsUpload(storedFileName);
		}

		public async Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDateTime)
		{
			var list = await Task.Run(() => _db.GetFileUploads(uploadDateTime, 1, null));
			return list;
		}

		public IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId)
		{
			return _db.GetFileUploads(date, uploadTypeId, statusId);
		}

		public FileUpload GetFileUpload(int id)
		{
			return _db.GetFileUpload(id);
		}




		/// <summary>
		/// Reads uploaded files one by one and imports them to DailyPrices table
		/// - Picks files with Status 1 = Uploaded
		/// - Sets status 5 = Processing, Reads thru file and adds records to DP,
		/// - Sets FileUpload status to 10 Success or 15 if any error at all
		/// - NEW DeleteRecordsForOlderImportsOfDate (yet to test)
		/// We stop at the first successful file since we should only process the latest files (no-brainer)
		/// </summary>
		/// <param name="listOfFiles"></param>
		/// <returns></returns>
		public FileUpload ProcessDailyPrice(List<FileUpload> listOfFiles)
		{
			listOfFiles = listOfFiles.OrderByDescending(x => x.UploadDateTime).ToList(); // start processing with the most recent file first
			FileUpload retval = null;

			foreach (FileUpload aFile in listOfFiles)
			{
				retval = aFile;
				_db.UpdateImportProcessStatus(5, aFile);//Processing 5
				var storedFilePath = _appSettings.UploadPath;
				var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
				int lineNumber = 0;
				bool hasWarning = false;
				try
				{
					List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();

					using (var file = new StreamReader(filePathAndName.ToString(CultureInfo.InvariantCulture)))
					{
						while (file.Peek() >= 0)
						{
							lineNumber++;

							string line = file.ReadLine();
                            if (line.Trim() == "") continue;
							try
							{
								var newDailyPrice = parseDailyLineValues(line, lineNumber, aFile);

								listOfDailyPricePrices.Add(newDailyPrice);
							}
							catch (Exception ex)
							{
								//log error and continue loading file
								var message = string.Format("Unable to parse daily prices file line: {0}. Fix this line data and try again or contact support team.", lineNumber);

								_db.LogImportError(aFile,
									message,
									lineNumber);

								_db.LogImportError(aFile,
									ex,
									lineNumber);

								hasWarning = true;
							}
						}
					}

					List<bool> importStatus = new List<bool>();

					//reset linNumber to 0
					lineNumber = 0;

					while (lineNumber < listOfDailyPricePrices.Count)
					{
						try
						{
							var nextBatch = listOfDailyPricePrices.Skip(lineNumber).Take(Constants.DailyFileRowsBatchSize).ToList();

							importStatus.Add(_db.NewDailyPrices(nextBatch, aFile, lineNumber));
						}
						catch (Exception ex)
						{
							throw new DailyFileNewBatchException(string.Format("Unable to load daily prices batch number: {0}. Contact support team.", lineNumber), ex);
						}
						lineNumber += Constants.DailyFileRowsBatchSize;
					}

					aFile.StatusId = importStatus.All(c => c)
						? (int)ImportProcessStatuses.Success
						: (int)ImportProcessStatuses.Failed;

					if (hasWarning && aFile.StatusId == (int)ImportProcessStatuses.Success)
						aFile.StatusId = (int)ImportProcessStatuses.Warning; // Warning

					_db.UpdateImportProcessStatus(aFile.StatusId, aFile);    
                     
					
				}
				catch (Exception ex)
				{
					_db.LogImportError(aFile, ex, lineNumber);

					_db.UpdateImportProcessStatus(15, aFile);

					return null;
				}
			}
			return retval;
		}

		//Process Quarterly File//////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Picks the right file to Process and returns it
		/// </summary>
		/// <param name="uploadedFiles"></param>
		/// <returns>The file picked for processing (only one)</returns>
		public FileUpload ProcessQuarterlyFileNew(List<FileUpload> uploadedFiles)
		{
			if (!uploadedFiles.Any())
				return null;

			var aFile = uploadedFiles.OrderByDescending(x => x.UploadDateTime).ToList().First();

			try
			{
				aFile.StatusId = (int)ImportProcessStatuses.Success;

				_db.UpdateImportProcessStatus(5, aFile); //Processing 5

				var rows = getXlsDataRows(aFile);

				var dataRows = rows as IList<DataRow> ?? rows.ToList();

				if (!dataRows.Any())
				{
					throw new ApplicationException(string.Format("No rows found in the file: {0}; Date: {1}; Contact support team.",
										aFile.OriginalFileName, aFile.UploadDateTime));
				}

				// Delete older rows before import
				_db.TruncateQuarterlyUploadStaging();

				bool gotWarning = false;

				var success = importQuarterlyRecordsToStaging(aFile, dataRows, out gotWarning); // dumps all rows into the quarterly staging table

				if (!success)
				{
					throw new ImportQuarterlyRecordsToStagingException("Unable to populate staging table in db. Contact support team.");
				}

				if (gotWarning)
					aFile.StatusId = (int)ImportProcessStatuses.Warning;

				var newQuarterlyRecords = _db.GetQuarterlyRecords();



				try
				{
					var sitesToUpdateCatNo = updateExistingSainsburysSitesWithNewCatalistNo(newQuarterlyRecords);

					_db.UpdateSitesCatNo(sitesToUpdateCatNo);
				}
				catch (Exception ex)
				{
					throw new CatalistNumberUpdateException("Unable update Catalist Numbers. Contact support team.", ex);
				}



                //Adding All Competitors Sites 
				try
				{
					var newSitesToAdd = addNewSites(newQuarterlyRecords);

					_db.NewSites(newSitesToAdd);

				}
				catch (Exception ex)
				{
					throw new NewSiteException("Unable to add new Sites. Contact support team.", ex);
				}

                //Adding new Sainsburys Sites if any
                try
				{
                    var newSitesToAdd = addNewSainsburysSites(newQuarterlyRecords);

					_db.NewSites(newSitesToAdd);

				}
				catch (Exception ex)
				{
					throw new NewSiteException("Unable to add new sainsburys Sites. Contact support team.", ex);
				}

                

				try
				{
					var sitesToUpdateByCatNo = updateExistingSitesWithNewDataByCatNo(newQuarterlyRecords);

					_db.UpdateSitesPrimaryInformation(sitesToUpdateByCatNo);
				}
				catch (Exception ex)
				{
					throw new UpdateSiteException("Unable to update Sites details. Contact support team.", ex);
				}

				try
				{
					var newSiteToCompetitorRecords = getNewSiteToCompetitors(newQuarterlyRecords);

					_db.UpdateSiteToCompetitor(newSiteToCompetitorRecords);
				}
				catch (Exception ex)
				{
					throw new NewSiteToCompetitorException("Unable to create Site to Competitors records. Contact support team.", ex);
				}

				_db.UpdateImportProcessStatus(aFile.StatusId, aFile); //ok 10, failed 15
			}
			catch (ExcelParseFileException ex)
			{
				_db.LogImportError(aFile, ex.Message, null);
				_db.UpdateImportProcessStatus(15, aFile); //failed 15
				return null;
			}
			catch (Exception ex)
			{
				_db.LogImportError(aFile, ex, null);

				_db.UpdateImportProcessStatus(15, aFile); //failed 15
				return null;
			}
			return aFile;
		}


        //Process Quarterly File//////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Picks the right file to Process and returns it
        /// </summary>
        /// <param name="uploadedFiles"></param>
        /// <returns>The file picked for processing (only one)</returns>
        public FileUpload ProcessLatestPriceFileNew(List<FileUpload> uploadedFiles)
        {
            if (!uploadedFiles.Any())
                return null;

            var aFile = uploadedFiles.OrderByDescending(x => x.UploadDateTime).ToList().First();

            try
            {
                aFile.StatusId = (int)ImportProcessStatuses.Success;

                _db.UpdateImportProcessStatus(5, aFile); //Processing 5

                var rows = getXlsDataRowsLatestSiteData(aFile);

                var dataRows = rows as IList<DataRow> ?? rows.ToList();

                if (!dataRows.Any())
                {
                    throw new ApplicationException(string.Format("No rows found in the file: {0}; Date: {1}; Contact support team.",
                                        aFile.OriginalFileName, aFile.UploadDateTime));
                }

                // Delete older rows before import
                _db.TruncateLatestPriceData();

                bool gotWarning = false;

                var success = importLatestPricRecords(aFile, dataRows, out gotWarning); // dumps all rows into the quarterly staging table

                if (!success)
                {
                    throw new ImportLatestSitePriceDataException("Unable to populate Latest Price Data table in db. Contact support team.");
                }

                if (gotWarning)
                    aFile.StatusId = (int)ImportProcessStatuses.Warning;
               

                _db.UpdateImportProcessStatus(aFile.StatusId, aFile); //ok 10, failed 15
            }
            catch (ExcelParseFileException ex)
            {
                _db.LogImportError(aFile, ex.Message, null);
                _db.UpdateImportProcessStatus(15, aFile); //failed 15
                return null;
            }
            catch (Exception ex)
            {
                _db.LogImportError(aFile, ex, null);

                _db.UpdateImportProcessStatus(15, aFile); //failed 15
                return null;
            }
            return aFile;
        }
		public void CleanupIntegrationTestsData(string testUserName = "Integration tests")
		{
			_db.CleanupIntegrationTestsData(testUserName);
		}

		#region Private Methods
		private List<Site> updateExistingSainsburysSitesWithNewCatalistNo(IEnumerable<QuarterlyUploadStaging> allQuarterlyRecords)
		{
			var jsSitesWithoutCatNo = _db.GetSites()
				.Where(s => s.IsSainsburysSite && s.CatNo.HasValue == false)
				.ToDictionary(k => k.SiteName, v => v);

			var jsSiteNamesWithoutCatNo = jsSitesWithoutCatNo.Select(js => js.Key).Distinct().ToArray();

			var newSainsburysSitesWithCatNo = allQuarterlyRecords
				.Where(qr => qr.Brand.ToUpperInvariant().Equals(Const.SAINSBURYS)
				&& jsSiteNamesWithoutCatNo.Contains(qr.SiteName));

			var result = new List<Site>();

			foreach (var newSainsburysSiteWithCatNo in newSainsburysSitesWithCatNo)
			{
				var siteToUpdate = jsSitesWithoutCatNo[newSainsburysSiteWithCatNo.SiteName];
				siteToUpdate.CatNo = newSainsburysSiteWithCatNo.CatNo;

				if (false == result.Any(r => r.CatNo == newSainsburysSiteWithCatNo.CatNo))
				{
					result.Add(siteToUpdate);
				}
			}

			return result;
		}

		private List<Site> addNewSites(IEnumerable<QuarterlyUploadStaging> allQuarterlyRecords)
		{
			var allSites = _db.GetSites();

			var allExistingCatNo = allSites
				.Where(s => s.CatNo.HasValue)
				.Select(s => s.CatNo)
				.Distinct()
				.ToArray();

			var newSiteRecords = allQuarterlyRecords
				.Where(ns => allExistingCatNo.Contains(ns.CatNo) == false)
				.GroupBy(g => g.CatNo)
				.Select(s => s.First());

			var result = new List<Site>();

			foreach (var newSiteRecord in newSiteRecords)
			{
				result.Add(newSiteRecord.ToSite());
			}

			return result;
		}


        private List<Site> addNewSainsburysSites(IEnumerable<QuarterlyUploadStaging> allQuarterlyRecords)
        {
            var allSites = _db.GetSites();

            var allExistingCatNo = allSites
                .Where(s => s.CatNo.HasValue)
                .Select(s => s.CatNo)
                .Distinct()
                .ToArray();

            var newSiteRecords = allQuarterlyRecords
                .Where(ns => allExistingCatNo.Contains(ns.SainsSiteCatNo) == false)
                .Select(s => new { s.SainsSiteName,s.SainsSiteTown, s.SainsSiteCatNo}).Distinct();

            var result = new List<Site>();

            foreach (var newSiteRecord in newSiteRecords)
            {
                Site site=new Site();
                site.CatNo = newSiteRecord.SainsSiteCatNo;
                site.Brand = Const.SAINSBURYS;
                site.SiteName = newSiteRecord.SainsSiteName;
                site.Town = newSiteRecord.SainsSiteTown;
                site.IsSainsburysSite = true;
                site.IsActive = true;
                site.Address = "";
                site.Suburb = "";
                site.PostCode = "";
                site.Company = "J SAINSBURY PLC";
                site.Ownership = "";
              
              //  site.StoreNo = newSiteRecord.SainsSiteCatNo;
                result.Add(site);
            }

            return result;
        }

		private List<Site> updateExistingSitesWithNewDataByCatNo(IEnumerable<QuarterlyUploadStaging> allQuarterlyRecords)
		{
			var allExistingSitesWithCatNo = _db.GetSites()
				.Where(s => s.CatNo.HasValue)
				.GroupBy(g => g.CatNo)
				.Select(g => g.First())
				.ToDictionary(k => k.CatNo.Value, v => v);

			List<Site> result = new List<Site>();

			foreach (var quarterlyRecord in allQuarterlyRecords)
			{
				if (allExistingSitesWithCatNo.ContainsKey(quarterlyRecord.CatNo))
				{
					var existingSite = allExistingSitesWithCatNo[quarterlyRecord.CatNo];

					var existingSiteWithNewValues = quarterlyRecord.ToSite();

					if (existingSite.ToHashCode() != existingSiteWithNewValues.ToHashCode()
						&& false == result.Any(r => r.CatNo == existingSiteWithNewValues.CatNo))
					{
						existingSiteWithNewValues.IsActive = existingSite.IsActive;
						existingSiteWithNewValues.IsSainsburysSite = existingSite.IsSainsburysSite;

						result.Add(existingSiteWithNewValues);
					}
				}
			}

			return result;
		}

		private List<SiteToCompetitor> getNewSiteToCompetitors(IEnumerable<QuarterlyUploadStaging> allQuarterlyRecords)
		{
			var allExistingSites = _db.GetSites()
				.Where(s => s.CatNo.HasValue)
				.GroupBy(g => g.CatNo)
				.Select(g => g.First())
				.ToDictionary(k => k.CatNo.Value, v => v);

			var result = new List<SiteToCompetitor>();

			foreach (var quarterlyRecord in allQuarterlyRecords)
			{
				Site jsSite = allExistingSites.ContainsKey(quarterlyRecord.SainsSiteCatNo) ? allExistingSites[quarterlyRecord.SainsSiteCatNo] : null;

				Site compSite = allExistingSites.ContainsKey(quarterlyRecord.CatNo) ? allExistingSites[quarterlyRecord.CatNo] : null;

				if (jsSite != null && compSite != null)
				{
					var newRecord = new SiteToCompetitor
					{
						SiteId = jsSite.Id,
						CompetitorId = compSite.Id,
						Rank = quarterlyRecord.Rank,
						DriveTime = quarterlyRecord.DriveTime,
						Distance = quarterlyRecord.DriveDist
					};

					result.Add(newRecord);
				}
			}

			return result;
		}

		// Reads XLS file and returns Rows
		private IEnumerable<DataRow> getXlsDataRows(FileUpload aFile)
		{
			using (DataTable dataTable = getQuarterlyData(aFile))
			{
				var rows = dataTable.ToDataRowsList();

				return rows;
			}
		}

        // Reads XLS file and returns Rows
        private IEnumerable<DataRow> getXlsDataRowsLatestSiteData(FileUpload aFile)
        {
            using (DataTable dataTable = getLatestPriceData(aFile))
            {
                var rows = dataTable.ToDataRowsList();

                return rows;
            }
        }

		/// <summary>
		/// Dumps all quarterly file xls records to Staging table in Batches
		/// </summary>
		/// <param name="aFile"></param>
		/// <param name="allRows"></param>
		/// <returns></returns>
		private bool importQuarterlyRecordsToStaging(FileUpload aFile, IEnumerable<DataRow> allRows, out bool hasWarning)
		{
			int batchNo = 0;
			hasWarning = false;

			foreach (IEnumerable<DataRow> batchRows in allRows.Batch(Constants.QuarterlyFileRowsBatchSize))
			{
				bool gotWarning = false;

				List<CatalistQuarterly> allSites = parseSiteRowsBatch(aFile, batchRows, batchNo, out gotWarning);

				if (gotWarning)
					hasWarning = true;

				var batchSuccess =
					_db.NewQuarterlyRecords(allSites, aFile, batchNo * Constants.QuarterlyFileRowsBatchSize);

				if (!batchSuccess)
				{
					return false;
				}

				batchNo += 1;
			}
			return true;
		}

        private bool importLatestPricRecords(FileUpload aFile, IEnumerable<DataRow> allRows, out bool hasWarning)
        {
            int batchNo = 0;
         
            List<LatestPriceDataModel> allSites = parseLatestPrice(aFile, allRows, out hasWarning);
            if (hasWarning) return false;
            var batchSuccess =
                    _db.NewLatestPriceRecords(allSites, aFile, 2);

               
           
            return true;
        }
		private DataTable getQuarterlyData(FileUpload aFile)
		{
			var storedFilePath = _appSettings.UploadPath;
			var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
         	return _dataFileReader.GetQuarterlyData(filePathAndName, _appSettings.ExcelFileSheetName);
		}

        private DataTable getLatestPriceData(FileUpload aFile)
        {
            var storedFilePath = _appSettings.UploadPath;
            var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
            return _dataFileReader.GetQuarterlyData(filePathAndName, "");
        }

		private List<CatalistQuarterly> parseSiteRowsBatch(FileUpload aFile, IEnumerable<DataRow> batchRows, int batchNo, out bool hasWarnings)
		{
			List<CatalistQuarterly> siteCatalistData = new List<CatalistQuarterly>();

			hasWarnings = false;

			int rowCount = 0;

			//starting from 2 to avoid headings held in row 1
			foreach (DataRow row in batchRows)
			{
				rowCount++;

				CatalistQuarterly site = new CatalistQuarterly();

				try
				{
					//Sainsburys Store
					site.SainsSiteName = row[0].ToString();
					site.SainsSiteTown = row[1].ToString();
					site.SainsCatNo = double.Parse(row[2].ToString()); // has to be a valid value, else file invalid

					//Site to competitor
					site.Rank = String.IsNullOrEmpty(row[3].ToString()) ? 999d : double.Parse(row[3].ToString()); // forgiving parse, set a high rank if not given
					site.DriveDistanceMiles = String.IsNullOrEmpty(row[4].ToString()) ? 999d : double.Parse(row[4].ToString()); // forgiving parse, set to a high value if not given
					site.DriveTimeMins = String.IsNullOrEmpty(row[5].ToString()) ? 999d : double.Parse(row[5].ToString()); // forgiving parse, set to a high value if not given

					//Competitiors Store
					site.CatNo = double.Parse(row[6].ToString()); // has to be a valid value, else file invalid
					site.Brand = row[7].ToString();
					site.SiteName = row[8].ToString();
					site.Address = row[9].ToString();
					site.Suburb = row[10].ToString();
					site.Town = row[11].ToString();
					site.Postcode = row[12].ToString();
					site.CompanyName = row[13].ToString();
					site.Ownership = row[14].ToString();

					siteCatalistData.Add(site);
				}
				catch (Exception ex)
				{
					//log error and continue loading file
					var message = string.Format("Unable to parse line from Catalist Quarterly File - line {0}. SainsCatNo: {1}. CatNo: {2}. SainsSiteName: {3}. SiteName: {4}",
						(batchNo * Constants.QuarterlyFileRowsBatchSize) + rowCount,
						row[2].ToString(),
						row[6].ToString(),
						row[0].ToString(),
						row[8].ToString());

					_db.LogImportError(aFile,
						message,
						rowCount);

					_db.LogImportError(aFile,
						ex,
						rowCount);

					hasWarnings = true;
				}

			}

			return siteCatalistData;
		}

        private List<LatestPriceDataModel> parseLatestPrice(FileUpload aFile, IEnumerable<DataRow> batchRows,  out bool hasWarnings)
        {
            List<LatestPriceDataModel> priceLatestPriceData = new List<LatestPriceDataModel>();

            hasWarnings = false;

            int rowCount = 0;

            //starting from 2 to avoid headings held in row 1
            foreach (DataRow row in batchRows)
            {
                rowCount++;

                LatestPriceDataModel LatestPriceData = new LatestPriceDataModel();

                try
                {
                    if (row[0].ToString().ToUpper() == "PFS") continue;
                    //Sainsburys Store
                    LatestPriceData.PfsNo = Convert.ToInt32(row[0].ToString());
                    LatestPriceData.StoreNo = Convert.ToInt32(row[1].ToString());
                    LatestPriceData.SiteName = row[2].ToString();
                    LatestPriceData.UnleadedPrice = row[3].ToString();
                    LatestPriceData.SuperUnleadedPrice = row[4].ToString();
                    LatestPriceData.DieselPrice = row[6].ToString();


                    priceLatestPriceData.Add(LatestPriceData);
                }
                catch (Exception ex)
                {
                    //log error and continue loading file
                    _db.LogImportError(aFile,
                        ex,
                        rowCount);

                    hasWarnings = true;
                }

            }

            return priceLatestPriceData;
        }

		/// <summary>
		/// Parses the CSV line to make a DailyPrice object
		/// - Logs error if parsing fails
		/// </summary>
		/// <param name="lineValues"></param>
		/// <param name="lineNumber"></param>
		/// <param name="aFile"></param>
		/// <returns>DailyPrice or throws exception</returns>
		private DailyPrice parseDailyLineValues(string lineValues, int lineNumber, FileUpload aFile)
		{
			string[] words = lineValues.Split(',');

			DailyPrice result = new DailyPrice
			{

				DailyUpload = aFile,
				// forgiving parse, set a CatNo which wont show up in calc
				CatNo = (String.IsNullOrEmpty(words[0]) ? -1 : int.Parse(words[0])),
				// has to be a valid value
				FuelTypeId = int.Parse(words[1]),
				// forgiving parse, since system doesnt use it
				AllStarMerchantNo = (String.IsNullOrEmpty(words[2]) ? 0 : int.Parse(words[2])),
				// YMD format, works across cultures, // has to be a valid value
				DateOfPrice = DateTime.Parse(words[3].Substring(0, 4) + "-" + words[3].Substring(4, 2) + "-" + words[3].Substring(6, 2)),
				// has to be a valid value
				ModalPrice = int.Parse(words[10])
			};

			return result;
		}

		/// <summary>
		/// Checks if any DailyFile available, then Fires OFF the calc to run..
		/// </summary>
		/// <param name="fileProcessed"></param>
		/// <returns></returns>
		private void runRecalc(FileUpload fileProcessed)
		{
			// Now see if any File available for calc and kickoff calc if yes..
			var dpFile = _db.GetDailyFileAvailableForCalc(fileProcessed.UploadDateTime);

			if (dpFile != null)
			{
				_priceService.DoCalcDailyPrices(fileProcessed.UploadDateTime);
			}
		}
		#endregion

	}
}