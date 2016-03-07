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

		private readonly ISettingsService _settingsService;

		private readonly IPetrolPricingRepository _db;

		private readonly IDataFileReader _dataFileReader;

		public FileService(IPetrolPricingRepository db,
			IPriceService priceService,
			ISettingsService settingsService,
			IDataFileReader dataFileReader)
		{
			_db = db;
			_priceService = priceService;
			_settingsService = settingsService;
			_dataFileReader = dataFileReader;
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
						throw new Exception("Upload failed..");

					runRecalc(processedFile);

					break;
				case 2:
					processedFile = ProcessQuarterlyFileNew(newUploadList);

					if (processedFile == null)
						throw new Exception("Upload failed..");

					runRecalc(processedFile);

					break;
				default:
					throw new ApplicationException("Not a valid File Type to import:" + newUpload.UploadTypeId);
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
				var storedFilePath = _settingsService.GetUploadPath();
				var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
				try
				{
					List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();

					int lineNumber = 0;

					using (var file = new StreamReader(filePathAndName.ToString(CultureInfo.InvariantCulture)))
					{
						while (file.Peek() >= 0)
						{
							lineNumber++;

							string line = file.ReadLine();

							var newDailyPrice = parseDailyLineValues(line, lineNumber, aFile);

							listOfDailyPricePrices.Add(newDailyPrice);
						}
					}

					List<bool> importStatus = new List<bool>();

					lineNumber = 0;

					while (lineNumber < listOfDailyPricePrices.Count)
					{
						var nextBatch = listOfDailyPricePrices.Skip(lineNumber).Take(Constants.DailyFileRowsBatchSize).ToList();

						importStatus.Add(_db.NewDailyPrices(nextBatch, aFile, lineNumber));

						lineNumber += Constants.DailyFileRowsBatchSize;
					}

					aFile.StatusId = importStatus.All(c => c) ? 10 : 15;

					_db.UpdateImportProcessStatus(aFile.StatusId, aFile);

					if (aFile.StatusId == 10)
					{
						// We clear out the dailyPrices for older imports and keep ONLY Latest set of DailyPrices
						// Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
						_db.DeleteRecordsForOlderImportsOfDate(DateTime.Today, aFile.Id);
						// Exit on first Successful Calc
						break; // exit foreach 
					}
				}
				catch (Exception ex)
				{
					_db.LogImportError(aFile, ex.Message + " filePath=" + filePathAndName, null);
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
				_db.UpdateImportProcessStatus(5, aFile); //Processing 5

				var rows = getXlsDataRows(aFile);

				var dataRows = rows as IList<DataRow> ?? rows.ToList();

				if (!dataRows.Any())
				{
					throw new Exception("No rows found in file:" + aFile.OriginalFileName + " dated:" +
										aFile.UploadDateTime);
				}

				// Delete older rows before import
				_db.DeleteRecordsForQuarterlyUploadStaging();

				var success = importQuarterlyRecordsToStaging(aFile, dataRows); // dumps all rows into the quarterly staging table

				if (!success)
				{
					throw new Exception("Unable to populate staging table in db");
				}

				// RUN sprocs to Add/Update/Delete sites and siteToCompetitors 
				_db.ImportQuarterlyUploadStaging(aFile.Id);

				aFile.StatusId = 10;

				_db.UpdateImportProcessStatus(aFile.StatusId, aFile); //ok 10, failed 15
			}
			catch (OleDbException ex)
			{
				if(ex.Message.Contains("Make sure that it does not include invalid characters or punctuation and that it is not too long."))
				{
					var message = string.Format("Invalid Sheet 1 name. Expected name: {0}", _settingsService.ExcelFileSheetName());
					_db.LogImportError(aFile, message, null);
					_db.UpdateImportProcessStatus(15, aFile); //failed 15
					return null;
				}
				else
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				_db.LogImportError(aFile, ex.Message, null);
				_db.UpdateImportProcessStatus(15, aFile); //failed 15
				return null;
			}
			return aFile;
		}

		#region Private Methods
		// Reads XLS file and returns Rows
		private IEnumerable<DataRow> getXlsDataRows(FileUpload aFile)
		{
			using (DataTable dataTable = getQuarterlyData(aFile))
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
		private bool importQuarterlyRecordsToStaging(FileUpload aFile, IEnumerable<DataRow> allRows)
		{
			int batchNo = 0;
			foreach (IEnumerable<DataRow> batchRows in allRows.Batch(Constants.QuarterlyFileRowsBatchSize))
			{
				List<CatalistQuarterly> allSites = parseSiteRowsBatch(aFile, batchRows, batchNo);
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

		private DataTable getQuarterlyData(FileUpload aFile)
		{
			var storedFilePath = _settingsService.GetUploadPath();
			var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);

			return _dataFileReader.GetQuarterlyData(filePathAndName, _settingsService.ExcelFileSheetName());
		}

		private List<CatalistQuarterly> parseSiteRowsBatch(FileUpload aFile, IEnumerable<DataRow> batchRows, int batchNo)
		{
			List<CatalistQuarterly> siteCatalistData = new List<CatalistQuarterly>();
			int rowCount = 0;
			//starting from 2 to avoid headings held in row 1
			foreach (DataRow row in batchRows)
			{
				rowCount++;
				try
				{
					CatalistQuarterly site = new CatalistQuarterly();

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
					//log error and continue..
					_db.LogImportError(aFile,
						ex.Message + string.Format(" --> Unable to add/parse line from Catalist Quarterly File - line {0}. Values 1: {1}. Value 2: {2}. JS name: {3}. Site name: {4}. Row to string: {5}", (batchNo * Constants.QuarterlyFileRowsBatchSize) + rowCount, row[2].ToString(), row[6].ToString(), row[0].ToString(), row[8].ToString(), row.ToString()),
						rowCount);
					throw;
				}
			}

			return siteCatalistData;
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