using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;

using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

using Excel = Microsoft.Office.Interop.Excel;



using System.IO;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class FileService : BaseService, IDisposable
    {
        public FileUpload NewUpload(FileUpload fileUpload)
        {
            FileUpload newUpload = _db.NewUpload(fileUpload);

            var processedFiles = UpdateDailyPrice(GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, 1).ToList());
            var fileUploads = processedFiles as IList<FileUpload> ?? processedFiles.ToList();
            if (fileUploads.Any())
            {
                CalcSitePrices(fileUploads);
            }

            return newUpload;
        }

        /// <summary>
        /// Calculate prices for files Uploaded today and in a Success state. No retrosprctive calc, No future calc
        /// </summary>
        /// <param name="processedFiles"></param>
        private void CalcSitePrices(IEnumerable<FileUpload> processedFiles)
        {
            var priceService = new PriceService();
            var siteService = new SiteService(_db);
            var forDate = DateTime.Now;

            var sites = _db.GetSitesIncludePrices();
            var fuels = LookupService.GetFuelTypes().ToList();

            foreach (var processedFile in processedFiles)
            {
                // Only ones Uploaded today and successfully processed files
                if (processedFile.UploadDateTime.Equals(forDate) && processedFile.Status.Id == 10) 
                {
                    foreach (var site in sites)
                    {
                        var tmpSite = site;
                        foreach (var fuel in fuels.ToList())
                        {
                            var calculatedSitePrice = priceService.CalcPrice(site.Id, fuel.Id);
                            var updatedPrice = _db.AddOrUpdateSitePriceRecord(calculatedSitePrice);
                        }
                    }
                }
            }
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

        //Process Daily Files
        public IEnumerable<FileUpload> UpdateDailyPrice(List<FileUpload> listOfFiles)
        {
            foreach (FileUpload aFile in listOfFiles)
            {
                _db.UpdateImportProcessStatus(aFile, 5);//Processing 5
                var storedFilePath = SettingsService.GetUploadPath();
                var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);
                try
                {
                    string line;
                    int lineNumber = 0;
                    List<bool> importStatus = new List<bool>();
                    List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();
                    filePathAndName = ""; // FORCES Error

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
                    _db.UpdateImportProcessStatus(aFile, aFile.StatusId);

                    file.Close();
                }
                catch (Exception ex)
                {
                    _db.LogImportError(aFile, ex.Message + "filePath=" + filePathAndName, null);
                    _db.UpdateImportProcessStatus(aFile, 15);
                }
            }
            return listOfFiles;
        }

        private DailyPrice ParseDailyLineValues(string lineValues, int lineNumber, FileUpload aFile)
        {
            DailyPrice theDailyPrice = new DailyPrice();

            try
            {
                string[] words = lineValues.Split(',');

                theDailyPrice.DailyUpload = aFile;
                theDailyPrice.CatNo = int.Parse(words[0]);
                theDailyPrice.FuelTypeId = int.Parse(words[1]);
                theDailyPrice.AllStarMerchantNo = int.Parse(words[2]);
                theDailyPrice.ModalPrice = int.Parse(words[10]);
                theDailyPrice.DateOfPrice = DateTime.Parse(words[3].Substring(6, 2) + "/" + words[3].Substring(4, 2) + "/" + words[3].Substring(0, 4));
            }
            catch 
            {
                _db.LogImportError(aFile, "Unable to Parse line", lineNumber);
                return null;
            }
            return theDailyPrice;
        }

        //Process Quarterly File//////////////////////////////////////////////////////////////////////////////////////

        public bool ProcessQuarterlyFile()
        {
            //start test
            FileUpload aFile = new FileUpload();
            aFile.Id = 1;
            aFile.OriginalFileName = "20151126 163800hrs - Catalist quarterly data.xlsx";
            //end test

            List<CatalistQuarterly> catalistQuarterlyData = new List<CatalistQuarterly>();
            catalistQuarterlyData = GetQuarterlyFileValues(aFile);
            SaveCatalistQuarterlyData(catalistQuarterlyData);


            return true;
        }

        private bool SaveCatalistQuarterlyData(List<CatalistQuarterly> CatalistQuarterlyData)
        {
           
            return true;
        }

        private List<CatalistQuarterly> GetQuarterlyFileValues(FileUpload aFile)
        {

            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            string str;
            int rowCount = 0;
            int columCount = 0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(aFile.OriginalFileName, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;

            List<CatalistQuarterly> catalistQuarterlyData = new List<CatalistQuarterly>();

            //starting from 2 to avoid headings held in row 1
            for (rowCount = 2; rowCount <= range.Rows.Count; rowCount++)
            {

                //move try outside of loop
                try
                {
                    CatalistQuarterly site = new CatalistQuarterly();

                    site.MasterSiteName = (range.Cells[rowCount, 1] as Excel.Range).Value2;
                    site.SiteTown = (range.Cells[rowCount, 2] as Excel.Range).Value2;
                    site.SiteCatNo = (range.Cells[rowCount, 3] as Excel.Range).Value2;
                    site.Rank = (range.Cells[rowCount, 4] as Excel.Range).Value2;
                    site.DriveDistanceMiles = (range.Cells[rowCount, 5] as Excel.Range).Value2;
                    site.DriveTimeMins = (range.Cells[rowCount, 6] as Excel.Range).Value2;
                    site.CatNo = (range.Cells[rowCount, 7] as Excel.Range).Value2;
                    site.Brand = (range.Cells[rowCount, 8] as Excel.Range).Value2;
                    site.SiteName = (range.Cells[rowCount, 9] as Excel.Range).Value2;
                    site.Address = (range.Cells[rowCount, 10] as Excel.Range).Value2;
                    site.Suburb = (range.Cells[rowCount, 11] as Excel.Range).Value2;
                    site.Town = (range.Cells[rowCount, 12] as Excel.Range).Value2;
                    site.Postcode = (range.Cells[rowCount, 13] as Excel.Range).Value2;
                    site.CompanyName = (range.Cells[rowCount, 14] as Excel.Range).Value2;
                    site.Ownership = (range.Cells[rowCount, 15] as Excel.Range).Value2;

                    catalistQuarterlyData.Add(site);
                }
                catch
                {
                    _db.LogImportError(aFile, "Unable to add/parse line from Catalist Quarterly File - line " + rowCount, rowCount);
                    //return null;
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return catalistQuarterlyData;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                // MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        } 

        //TODO Move models project /view models
        public class CatalistQuarterly
        { 
            //Types are set as in excel to avoid parsing on import
            public string MasterSiteName {get; set;}
            public string SiteTown { get; set; }
            public double SiteCatNo { get; set; }
            public double Rank { get; set; }
            public double DriveDistanceMiles { get; set; }
            public double DriveTimeMins { get; set; }
            public double CatNo { get; set; }
            public string Brand { get; set; }
            public string SiteName { get; set; }
            public string Address { get; set; }
            public string Suburb { get; set; }
            public string Town { get; set; }
            public string Postcode { get; set; }
            public string CompanyName { get; set; }
            public string Ownership { get; set; }
        }
       
    }
}