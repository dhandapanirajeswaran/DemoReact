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
                string line;
                int lineNumber = 0;
                List<bool> importStatus = new List<bool>();
                List<DailyPrice> ListOfDailyPricePrices = new List<DailyPrice>();

                _db.UpdateImportProcessStatus(aFile, 5);//Processing 5
                StreamReader file = new StreamReader(aFile.StoredFileName.ToString());

                while ((line = file.ReadLine()) != null)
                {
                    lineNumber ++;
                    ListOfDailyPricePrices.Add(ParseDailyLineValues(line, lineNumber, aFile));

                    if (ListOfDailyPricePrices.Count == 100)
                    {
                        importStatus.Add(_db.NewDailyPrices(ListOfDailyPricePrices, aFile, lineNumber));
                        ListOfDailyPricePrices.Clear();
                    }
                }
                if (ListOfDailyPricePrices.Any())
                {
                    importStatus.Add(_db.NewDailyPrices(ListOfDailyPricePrices, aFile, lineNumber));
                    ListOfDailyPricePrices.Clear();
                }

                if (importStatus.All(c => c == true))
                {
                    _db.UpdateImportProcessStatus(aFile, 10);//Success 10
                }
                else
                {
                    _db.UpdateImportProcessStatus(aFile, 15);//Failed 15
                }

                file.Close();


            }
            return true;
        }

        private DailyPrice ParseDailyLineValues(string lineValues, int LineNumber, FileUpload aFile)
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
                _db.LogImportError(aFile, "Unable to Parse line", LineNumber);
            }
           
            return theDailyPrice;

           
        }

       



    }
}