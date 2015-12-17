using System;
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
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using Newtonsoft.Json.Serialization;

using Excel = Microsoft.Office.Interop.Excel;



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
                case 2: processedFiles = ProcessQuarterlyFile(GetFileUploads(newUpload.UploadDateTime, newUpload.UploadTypeId, uploadedStatus).ToList());
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

        public IEnumerable<FileUpload> ProcessQuarterlyFile(List<FileUpload> uploadedFiles)
        {
            throw new NotImplementedException();
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
            aFile.OriginalFileName = "C:/Temp/20151126 163800hrs - Catalist quarterly data.xlsx";
            //end test

            //process Quarterly data
            return GetQuarterlyData(aFile);

        }

        //private bool SaveQuarterlyData(List<CatalistQuarterly> CatalistQuarterlyData, FileUpload aFile)
        //{

        //    _db.UpdateImportProcessStatus(aFile, 5);//Processing 5

        //    var storedFilePath = SettingsService.GetUploadPath();
        //    var filePathAndName = Path.Combine(storedFilePath, aFile.StoredFileName);

        //    try
        //    {
                
        //    }
        //    catch (Exception ex)
        //    {
        //        _db.LogImportError(aFile, ex.Message + "filePath=" + filePathAndName, null);
        //        _db.UpdateImportProcessStatus(aFile, 15);
        //        return false;
        //    }
         

        //    return true;
        //}


        private bool ProccessSiteData(Excel.Range Range, FileUpload aFile)
        { 
        
            Excel.Range range = Range;
            int rowCount = 0;
   
            List<CatalistQuarterly> siteCatalistData = new List<CatalistQuarterly>();


            //starting from 2 to avoid headings held in row 1
            for (rowCount = 2; rowCount <= range.Rows.Count; rowCount++)
            {
                try
                {
                    CatalistQuarterly site = new CatalistQuarterly();

                    //Sainsburys Store
                    site.MasterSiteName = (range.Cells[rowCount, 1] as Excel.Range).Value2;
                    site.SiteTown = (range.Cells[rowCount, 2] as Excel.Range).Value2;
                    site.SiteCatNo = (range.Cells[rowCount, 3] as Excel.Range).Value2;

                    //Competitiors Store
                    site.CatNo = (range.Cells[rowCount, 7] as Excel.Range).Value2;
                    site.Brand = (range.Cells[rowCount, 8] as Excel.Range).Value2;


                    site.SiteName = (range.Cells[rowCount, 9] as Excel.Range).Value2;
                    site.Address = (range.Cells[rowCount, 10] as Excel.Range).Value2;
                    site.Suburb = (range.Cells[rowCount, 11] as Excel.Range).Value2;
                    site.Town = (range.Cells[rowCount, 12] as Excel.Range).Value2;
                    site.Postcode = (range.Cells[rowCount, 13] as Excel.Range).Value2;
                    site.CompanyName = (range.Cells[rowCount, 14] as Excel.Range).Value2;
                    site.Ownership = (range.Cells[rowCount, 15] as Excel.Range).Value2;

                    //Site to competitor
                    site.Rank = (range.Cells[rowCount, 4] as Excel.Range).Value2;
                    site.DriveDistanceMiles = (range.Cells[rowCount, 5] as Excel.Range).Value2;
                    site.DriveTimeMins = (range.Cells[rowCount, 6] as Excel.Range).Value2;

                    siteCatalistData.Add(site);

                }
                catch
                {
                    _db.LogImportError(aFile, "Unable to add/parse line from Catalist Quarterly File - line " + rowCount, rowCount);
                    
                }
    }



            List<CatalistQuarterly> allUniqueSites = new List<CatalistQuarterly>();

            allUniqueSites = siteCatalistData.GroupBy(Site => Site.CatNo)
                  .Select(CatNo => CatNo.First())
                  .ToList();

            //Udates or Adds Sites from cat file
            _db.UpdateCatalistQuarterlyData(allUniqueSites, aFile, false);

            UpdateSiteToCompetitiorData(siteCatalistData);
           


            return true;

        }

        private void UpdateSiteToCompetitiorData (List<CatalistQuarterly> SiteCatalistData)
        {

            _db.UpdateSiteToCompFromQuarterlyData(SiteCatalistData);
            
            //List<CatalistQuarterly> siteCatalistData = new List<CatalistQuarterly>();
            //List<CatalistQuarterly> sainsburysSites = new List<CatalistQuarterly>();

            //siteCatalistData = SiteCatalistData;

            ////get all Sainsburys Sites from list
            //sainsburysSites = siteCatalistData.GroupBy(Site => Site.SiteCatNo)
            //.Select(SiteCatNo => SiteCatNo.First())
            //.ToList();

            //foreach (CatalistQuarterly site in sainsburysSites)
            //{
            //    List<CatalistQuarterly> ListOfCompetitorsForSainsburysSite = new List<CatalistQuarterly>();

            //    //Get Competitors for this Sainsburys site
            //    ListOfCompetitorsForSainsburysSite = siteCatalistData.GroupBy(Site => Site.SiteCatNo)
            //     .Select(SiteCatNo => site.CatNo)
            //     .ToList();

            //    List<SiteToCompetitor> ListOfCompetitor = new List<SiteToCompetitor>();

            //    //Compilers List of Sites To Competitor data for a Sainsburys site
            //    foreach(CatalistQuarterly Competitor in ListOfCompetitorsForSainsburysSite)
            //    {
            //        try
            //        {
            //            SiteToCompetitor siteToCompetitor = new SiteToCompetitor();

            //            //siteToCompetitor.Id = "";
            //            //siteToCompetitor.SiteId = "";
            //            //siteToCompetitor.CompetitorId = "";

            //            siteToCompetitor.Rank = int.Parse(Competitor.Rank.ToString());
            //            siteToCompetitor.DriveTime = float.Parse(Competitor.DriveTimeMins.ToString());
            //            siteToCompetitor.Distance = float.Parse(Competitor.DriveDistanceMiles.ToString());


            //            ListOfCompetitor.Add(siteToCompetitor);
            //        }
            //        catch
            //        {
            //            //TODO unable to added competitiors
            //        }
            //    }

            //    //AddUpdateSiteToCompetitor

            //    //add or update Competitor info

            //}


            //_db.UpdateCatalistQuarterlyData(SainsburysSites, aFile, true);
        }


        private bool GetQuarterlyData(FileUpload aFile)
        {
            

            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(aFile.OriginalFileName, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            ProccessSiteData(range, aFile);

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
           
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return true;
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

        
        
       
    }
}