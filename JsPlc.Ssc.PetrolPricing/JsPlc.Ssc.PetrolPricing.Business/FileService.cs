using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class FileService : IDisposable
    {
        public FileUpload NewUpload(FileUpload fileUpload)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.NewUpload(fileUpload);
            }
        }

        public void Dispose()
        {
            // do nothing for now
        }

        public void UpdateUpload(FileUpload fileUpload)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                db.UpdateUpload(fileUpload);
            }
        }

        public bool ExistsUpload(string storedFileName)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.ExistsUpload(storedFileName);
            }
        }

        public IEnumerable<FileUpload> ExistingDailyUploads(DateTime uploadDateTime)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetFileUploads(uploadDateTime, new UploadType{Id = 1}).ToList();
            }
        }

        public IEnumerable<FileUpload> GetFileUploads()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetFileUploads().ToList();
            }
        }
    }
}