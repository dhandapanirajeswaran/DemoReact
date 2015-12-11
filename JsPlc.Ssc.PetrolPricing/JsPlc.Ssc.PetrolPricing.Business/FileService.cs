using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

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
    }
}