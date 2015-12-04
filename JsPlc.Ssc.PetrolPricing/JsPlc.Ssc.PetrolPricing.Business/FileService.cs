using System;
using System.Collections.Generic;
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

            UpdateDailyPrice(GetFileUploads(null, null, 1));

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

        public bool UpdateDailyPrice(IEnumerable<FileUpload> ListOfFiles)
        {
            foreach (FileUpload aFile in ListOfFiles)
            {

                UpdateImportProcessStatus(aFile, 5);//Processing 5

                string line;
                StreamReader file = new StreamReader(aFile.StoredFileName.ToString());

                List<DailyPrice> ListOfDailyPricePrices = new List<DailyPrice>();

                while ((line = file.ReadLine()) != null)
                {
                    ListOfDailyPricePrices.Add(DailyLineValues(line, aFile));

                    if (ListOfDailyPricePrices.Count == 100)
                    {
                        _db.NewDailyPrices(ListOfDailyPricePrices, aFile);
                        ListOfDailyPricePrices.Clear();
                    }
                }
                if (ListOfDailyPricePrices.Any())
                {
                    _db.NewDailyPrices(ListOfDailyPricePrices, aFile);
                    ListOfDailyPricePrices.Clear();
                }

                UpdateImportProcessStatus(aFile, 10);//Success 10

                file.Close();


            }
            return true;
        }

        private DailyPrice DailyLineValues(string lineValues, FileUpload aFile)
        {
            string[] words = lineValues.Split(',');
            DailyPrice theDailyPrice = new DailyPrice();

            theDailyPrice.DailyUpload = aFile;
            theDailyPrice.CatNo = int.Parse(words[0]);
            theDailyPrice.FuelTypeId = int.Parse(words[1]);
            theDailyPrice.AllStarMerchantNo = int.Parse(words[2]);
            theDailyPrice.DateOfPrice = DateTime.Parse("11/11/2015");//DateTime.Parse(words[3]);
            theDailyPrice.ModalPrice = int.Parse(words[10]);

            return theDailyPrice;
        }

        private bool UpdateImportProcessStatus(FileUpload aFile, int statusId)
       {
            aFile.StatusId = statusId;
            _db.UpdateImportProcessStatus(aFile);
            return true;
        }



    }
}