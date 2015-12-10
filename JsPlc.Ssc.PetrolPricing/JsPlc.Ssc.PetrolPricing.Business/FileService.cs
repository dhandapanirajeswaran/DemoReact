﻿using System;
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

        public bool UpdateDailyPrice(IEnumerable<FileUpload> listOfFiles)
        {
            foreach (FileUpload aFile in listOfFiles)
            {
                string line;
                int lineNumber = 0;
                List<bool> importStatus = new List<bool>();
                List<DailyPrice> listOfDailyPricePrices = new List<DailyPrice>();

                _db.UpdateImportProcessStatus(aFile, 5);//Processing 5
                StreamReader file = new StreamReader(aFile.StoredFileName.ToString(CultureInfo.InvariantCulture));

                while ((line = file.ReadLine()) != null)
                {
                    lineNumber ++;
                    listOfDailyPricePrices.Add(ParseDailyLineValues(line, lineNumber, aFile));

                    if (listOfDailyPricePrices.Count != 100) continue;

                    importStatus.Add(_db.NewDailyPrices(listOfDailyPricePrices, aFile, lineNumber));
                    listOfDailyPricePrices.Clear();
                }
                if (listOfDailyPricePrices.Any())
                {
                    importStatus.Add(_db.NewDailyPrices(listOfDailyPricePrices, aFile, lineNumber));
                    listOfDailyPricePrices.Clear();
                }

                _db.UpdateImportProcessStatus(aFile, importStatus.All(c => c) ? 10 : 15);

                file.Close();


            }
            return true;
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
            }
           
            return theDailyPrice;

           
        }

       



    }
}