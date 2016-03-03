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

        public FileService(IPetrolPricingRepository db, 
            IPriceService priceService,
            ISettingsService settingsService)
        {
            _db = db;
            _priceService = priceService;
            _settingsService = settingsService;
        }

        public FileUpload NewUpload(FileUpload fileUpload)
        {
            FileUpload newUpload = _db.NewUpload(fileUpload);

            IEnumerable<FileUpload> filesToProcess;
            FileUpload processedFile;

            const int uploadedStatus = 1;
            
            // Use a fire and forget approach
            switch (newUpload.UploadTypeId)
            {
                case 1:
                    filesToProcess = GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus);
                    processedFile = ProcessDailyPrice(filesToProcess.ToList()); // rather quick

                    if (processedFile == null) 
                        throw new Exception("Upload failed..");
                    
                    RunRecalc(processedFile);
                    
                    break;
                case 2: 
                    filesToProcess = GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus);
                    processedFile = ProcessQuarterlyFileNew(filesToProcess.ToList());
                    
                    if (processedFile == null) 
                        throw new Exception("Upload failed..");

                    RunRecalc(processedFile);
                    
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
                    string line;
                    int lineNumber = 0;
                    List<bool> importStatus = new List<bool>();
                    List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();

                    using (var file = new StreamReader(filePathAndName.ToString(CultureInfo.InvariantCulture)))
                    {
                        bool success = true;
                        while ((line = file.ReadLine()) != null)
                        {
                            lineNumber++;
                            var dp = ParseDailyLineValues(line, lineNumber, aFile);
                            if (dp == null)
                            { success = false; importStatus.Add(false); break; }

                            listOfDailyPricePrices.Add(dp);

                            if (listOfDailyPricePrices.Count != 1000) continue;

                            success = _db.NewDailyPrices(listOfDailyPricePrices, aFile, lineNumber);

                            importStatus.Add(success);
                            listOfDailyPricePrices.Clear();

                            if (!success) break;
                        }
                        if (listOfDailyPricePrices.Any() && success)
                        {
                            importStatus.Add(_db.NewDailyPrices(listOfDailyPricePrices, aFile, lineNumber));
                            listOfDailyPricePrices.Clear();
                        }
                        aFile.StatusId = importStatus.All(c => c) ? 10 : 15;
                        _db.UpdateImportProcessStatus(aFile.StatusId, aFile);
                    }

                    if (aFile.StatusId == 10)
                    {
                        // We clear out the dailyPrices for older imports and keep ONLY Latest set of DailyPrices
                        // Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
                        _db.DeleteRecordsForOlderImportsOfDate(DateTime.Now, aFile.Id);
                        // Exit on first Successful Calc
                        break; // exit foreach 
                    }
                }
                catch (Exception ex)
                {
                    _db.LogImportError(aFile, ex.Message + "filePath=" + filePathAndName, null);
                    _db.UpdateImportProcessStatus(15, aFile);
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
            if (!uploadedFiles.Any()) return null;

            var latestFile = uploadedFiles.OrderByDescending(x => x.UploadDateTime).ToList().First();

            // start processing only the MOST recent file
            var aFile = latestFile;

            try
            {
                _db.UpdateImportProcessStatus(5, aFile); //Processing 5

                var rows = GetXlsDataRows(aFile);

                var dataRows = rows as IList<DataRow> ?? rows.ToList();
                if (!dataRows.Any())
                {
                    throw new Exception("No rows found in file:" + aFile.OriginalFileName + " dated:" +
                                        aFile.UploadDateTime);
                }

                // Delete older rows before import
                _db.DeleteRecordsForQuarterlyUploadStaging();
                
                var success = ImportQuarterlyRecordsToStaging(aFile, dataRows); // dumps all rows into the quarterly staging table
                if(!success)
                {
                    throw new Exception("Unable to populate staging table in db");
                }

                // RUN sprocs to Add/Update/Delete sites and siteToCompetitors 
                _db.ImportQuarterlyUploadStaging(aFile.Id);
                
                _db.UpdateImportProcessStatus(10, aFile); //ok 10, failed 15
            }
            catch (Exception ex)
            {
                _db.LogImportError(aFile, ex.Message, null);
                _db.UpdateImportProcessStatus(15, aFile); //failed 15
            }
            return aFile;
        }

        #region Private Methods
        // Reads XLS file and returns Rows
        private IEnumerable<DataRow> GetXlsDataRows(FileUpload aFile)
        {
            using (DataTable dataTable = GetQuarterlyData(aFile))
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
        private bool ImportQuarterlyRecordsToStaging(FileUpload aFile, IEnumerable<DataRow> allRows)
        {
            int batchNo = 0;
            foreach (IEnumerable<DataRow> batchRows in allRows.Batch(Constants.QuarterlyFileRowsBatchSize))
            {
                List<CatalistQuarterly> allSites = ParseSiteRowsBatch(aFile, batchRows, batchNo);
                var batchSuccess =
                    _db.NewQuarterlyRecords(allSites, aFile, batchNo*Constants.QuarterlyFileRowsBatchSize);
                if (!batchSuccess)
                {
                    return false;
                }

                batchNo += 1;
            }
            return true;
        }

        private DataTable GetQuarterlyData(FileUpload aFile)
        {
            var storedFilePath = _settingsService.GetUploadPath();
            var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);

            //REMOVE
            //var filePathAndName = "C:/Temp/20151126 163800hrs - Catalist quarterly data.xlsx";

            var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePathAndName + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'");

            using (var adapter = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}$]",
                _settingsService.ExcelFileSheetName()), connectionString))
            {
                using (var ds = new DataSet())
                {
                    adapter.Fill(ds, "x");
                    return ds.Tables[0].Copy();
                }
            }
        }

        private List<CatalistQuarterly> ParseSiteRowsBatch(FileUpload aFile, IEnumerable<DataRow> batchRows, int batchNo)
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
                catch(Exception ex)
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
        /// <returns>DailyPrice or null</returns>
        private DailyPrice ParseDailyLineValues(string lineValues, int lineNumber, FileUpload aFile)
        {
            DailyPrice theDailyPrice = new DailyPrice();

            try
            {
                string[] words = lineValues.Split(',');

                theDailyPrice.DailyUpload = aFile;
                theDailyPrice.CatNo = (String.IsNullOrEmpty(words[0]) ? -1 : int.Parse(words[0])); // forgiving parse, set a CatNo which wont show up in calc
                theDailyPrice.FuelTypeId = int.Parse(words[1]); // has to be a valid value
                theDailyPrice.AllStarMerchantNo = (String.IsNullOrEmpty(words[2]) ? 0 : int.Parse(words[2])); // forgiving parse, since system doesnt use it
                theDailyPrice.DateOfPrice = DateTime.Parse(words[3].Substring(0, 4) + "-" + words[3].Substring(4, 2) + "-" + words[3].Substring(6, 2)); // YMD format, works across cultures, // has to be a valid value

                theDailyPrice.ModalPrice = int.Parse(words[10]);  // has to be a valid value
            }
            catch
            {
                _db.LogImportError(aFile, "Unable to Parse line", lineNumber);
                return null;
            }
            return theDailyPrice;
        }

        /// <summary>
        /// Checks if any DailyFile available, then Fires OFF the calc to run..
        /// </summary>
        /// <param name="fileProcessed"></param>
        /// <returns></returns>
        //private async Task RunRecalc(FileUpload fileProcessed)
        private void RunRecalc(FileUpload fileProcessed)
        {
            // Now see if any File available for calc and kickoff calc if yes..
            var dpFile = _db.GetDailyFileAvailableForCalc(fileProcessed.UploadDateTime);
            if (dpFile != null)
            {
                //await _priceService.DoCalcDailyPrices(fileProcessed.UploadDateTime);
                _priceService.DoCalcDailyPrices(fileProcessed.UploadDateTime);
            }
        }
        #endregion

    }
}