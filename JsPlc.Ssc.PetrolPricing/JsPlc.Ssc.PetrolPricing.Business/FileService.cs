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
        public int NewUpload(FileUpload fileUpload)
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
    }
}