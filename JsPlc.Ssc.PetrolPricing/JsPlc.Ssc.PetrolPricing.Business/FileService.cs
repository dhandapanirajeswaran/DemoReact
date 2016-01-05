using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.Text;

using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using MoreLinq;
using Newtonsoft.Json.Serialization;

using System.Data.OleDb;



using System.IO;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class FileService : BaseService, IDisposable
    {
        private readonly PriceService _priceService = new PriceService();

        public async Task<FileUpload> NewUpload(FileUpload fileUpload)
        {
            FileUpload newUpload = _db.NewUpload(fileUpload);

            IEnumerable<FileUpload> processedFiles;
            const int uploadedStatus = 1;
            
            // Use a fire and forget approach
            switch (newUpload.UploadTypeId)
            {
                case 1: processedFiles = ProcessDailyPrice(GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus).ToList());
                    await _priceService.DoCalcDailyPrices(fileUpload.UploadDateTime); // dont await this.. let it run in background..
                    break;
                case 2: // LONG Running Task - Fire and Forget
                        //ProcessQuarterlyFile(GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus).ToList())
                        //    .Wait();
                    var filesToProcess = GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus);
                    var fileProcessed = await ProcessQuarterlyFileNew(filesToProcess.ToList());
                    // TODO what happens when we have new Quarterly file uploaded, do we calc prices
                    //_priceService.DoCalcPrices(fileUpload.UploadDateTime);
                    break;
                default: throw new ApplicationException("Not a valid File Type to import:" + newUpload.UploadTypeId);
            }
            return newUpload;
        }

        public void Dispose()
        {
            // do nothing for now
        }

        public bool ExistsUpload(string storedFileName)
        {
            return _db.ExistsUpload(storedFileName);
        }

        public IEnumerable<FileUpload> ExistingDailyUploads(DateTime uploadDateTime)
        {
            return _db.GetFileUploads(uploadDateTime, 1, null).ToList();
        }

        public IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId)
        {
            return _db.GetFileUploads(date, uploadTypeId, statusId).ToList();
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
        /// TODO: ideally we should stop at the first successful file since we should only process the latest files first
        /// </summary>
        /// <param name="listOfFiles"></param>
        /// <returns></returns>
        public IEnumerable<FileUpload> ProcessDailyPrice(List<FileUpload> listOfFiles)
        {
            listOfFiles = listOfFiles.OrderByDescending(x => x.UploadDateTime).ToList(); // start processing with the most recent file first

            foreach (FileUpload aFile in listOfFiles)
            {
                _db.UpdateImportProcessStatus(5, aFile);//Processing 5
                var storedFilePath = SettingsService.GetUploadPath();
                var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
                try
                {
                    string line;
                    int lineNumber = 0;
                    List<bool> importStatus = new List<bool>();
                    List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();
                    //filePathAndName = ""; // FORCES Error

                    var file = new StreamReader(filePathAndName.ToString(CultureInfo.InvariantCulture));
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

                    // If the latest upload imports successfully 
                    file.Close();

                    if (aFile.StatusId == 10)
                    {
                        // We clear out the dailyPrices for older imports and keep ONLY Latest set of DailyPrices
                        // Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
                        _db.DeleteRecordsForOlderImportsOfDate(DateTime.Now, aFile.Id);
                        // TODO Introduce switch to exit loop on first successful import. (not a requirement)
                    }
                }
                catch (Exception ex)
                {
                    _db.LogImportError(aFile, ex.Message + "filePath=" + filePathAndName, null);
                    _db.UpdateImportProcessStatus(15, aFile);
                }
            }
            return listOfFiles;
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

        //Process Quarterly File//////////////////////////////////////////////////////////////////////////////////////
        public async Task<FileUpload> ProcessQuarterlyFileNew(List<FileUpload> uploadedFiles)
        {
            if (!uploadedFiles.Any()) return uploadedFiles.FirstOrDefault();

            var latestFile = uploadedFiles.OrderByDescending(x => x.UploadDateTime).ToList().First();

            // start processing only the MOST recent file
            var aFile = latestFile;

            try
            {
                DataTable dataTable = await GetQuarterlyData(aFile);
                var rows = dataTable.ToDataRowsList();
                dataTable = null;
                if (!rows.Any())
                {
                    throw new Exception("No rows found in file:" + aFile.OriginalFileName + " dated:" +
                                        aFile.UploadDateTime);
                }

                _db.UpdateImportProcessStatus(5, aFile); //Processing 5

                // Delete older rows before import
                _db.DeleteRecordsForQuarterlyUploadStaging();

                var success = await ImportQuarterlyRecords(aFile, rows); // dumps all rows into the quarterly staging table

                // TODO ONCE this completes, RUN sprocs to Add/Update/Delete sites and siteToCompetitors 

                _db.UpdateImportProcessStatus(success? 10 : 15, aFile); //ok 10, failed 15
            }
            catch (Exception ex)
            {
                _db.LogImportError(aFile, ex.Message, null);
                _db.UpdateImportProcessStatus(15, aFile); //failed 15
            }
            return aFile;

        }

        private async Task<bool> ImportQuarterlyRecords(FileUpload aFile, IEnumerable<DataRow> allRows)
        {
            int batchNo = 0;
            foreach (IEnumerable<DataRow> batchRows in allRows.Batch(Constants.QuarterlyFileRowsBatchSize))
            {
                List<CatalistQuarterly> allSites = await ParseSiteRowsBatch(aFile, batchRows, batchNo);
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

        public async Task<IEnumerable<FileUpload>> ProcessQuarterlyFile(List<FileUpload> uploadedFiles)
        {
            foreach (FileUpload aFile in uploadedFiles)
            {
                try
                {
                    _db.UpdateImportProcessStatus(5, aFile); //Processing 5
                    await UpdateSitesFromCatalistQuarterly(aFile);
                    _db.UpdateImportProcessStatus(10, aFile);//ok 10
                }
                catch (Exception ex)
                {
                    _db.LogImportError(aFile, ex.Message, null);
                    _db.UpdateImportProcessStatus(15, aFile);//failed 15
                }
            }
            return uploadedFiles;
        }



        private async Task<bool> UpdateSitesFromCatalistQuarterly(FileUpload aFile)
        {
            //Get data from excel and parse to gen list
            DataTable dataTable = await GetQuarterlyData(aFile);

            var batchRows = new List<DataRow>();
            int rowcount = 0;
            int batchCount = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (DataRow row in dataTable.Rows)
            {
                if (rowcount > Constants.QuarterlyFileRowsBatchSize) // 1000 to start with
                {
                    List<DataRow> rows = batchRows;
                    int count = batchCount;
                    Task t = new Task(() => ProcessQuarterlyBatch(aFile, rows, count));
                    t.Start();
                    t.Wait();
                    

                    Debug.WriteLine("Batch of quarterly size:" + Constants.QuarterlyFileRowsBatchSize + " took: " + sw.ElapsedMilliseconds/1000 + " secs");
                    sw.Reset();
                    sw.Start();
                    batchRows = new List<DataRow>();
                    rowcount = 0;
                    batchCount += 1;
                    if (batchCount >= 1) return true; // only process 1 batches
                }
                batchRows.Add(row);
                rowcount += 1;
            }
            if (batchRows.Any())
                ProcessQuarterlyBatch(aFile, batchRows, batchCount);

            return true;
        }

        private void ProcessQuarterlyBatch(FileUpload aFile, IEnumerable<DataRow> batchRows, int batchNo)
        {
            List<CatalistQuarterly> allSites = new List<CatalistQuarterly>();
            List<CatalistQuarterly> allUniqueSites = new List<CatalistQuarterly>();

            allSites = ParseSiteRowsBatch(aFile, batchRows, batchNo).Result;

            allUniqueSites = allSites.GroupBy(site => site.CatNo)
                .Select(catNo => catNo.First())
                .ToList();

            //_db.UpdateImportProcessStatus(5, aFile);//Processing 5

            //Updates or Add Sites
            if (!_db.UpdateCatalistQuarterlyData(allUniqueSites, aFile, false))
            {
                throw new ApplicationException("Failed to update Site Data..");
                //_db.UpdateImportProcessStatus(15, aFile);//failed 15
            }
            //else { siteSuccess = true; }

            //Updates or Add Competitior info
            if (!_db.UpdateSiteToCompFromQuarterlyData(allSites))
            {
                throw new ApplicationException("Failed to update SiteToCompetitor Data..");
                //_db.UpdateImportProcessStatus(15, aFile);//failed 15
            }
        }

        private async Task<DataTable> GetQuarterlyData(FileUpload aFile)
        {
            DataTable data = new DataTable();
            var storedFilePath = SettingsService.GetUploadPath();
            var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);

            //REMOVE
            //var filePathAndName = "C:/Temp/20151126 163800hrs - Catalist quarterly data.xlsx";

            var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePathAndName + ";Extended Properties='Excel 12.0 Xml;HDR=YES'");
                
            var adapter = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}$]", SettingsService.GetSetting("ExcelQuarterlyFileSheetName")), connectionString);
            var ds = new DataSet();

            adapter.Fill(ds, "x");

            data = ds.Tables[0];

            return data;
        }

        private async Task<List<CatalistQuarterly>> ParseSiteRowsBatch(FileUpload aFile, IEnumerable<DataRow> batchRows, int batchNo)
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
                    _db.LogImportError(aFile, ex.Message +  " --> Unable to add/parse line from Catalist Quarterly File - line " + (batchNo * Constants.QuarterlyFileRowsBatchSize) + rowCount, rowCount);
                    // throw; // TODO fail on error
                }
            }

            return siteCatalistData;
        }


    }
}