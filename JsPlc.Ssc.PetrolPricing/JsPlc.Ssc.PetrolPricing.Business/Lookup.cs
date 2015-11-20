using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class Lookup
    {
        public IEnumerable<UploadType> GetUploadTypes()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetUploadTypes();
            }
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetFuelTypes();
            }
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetProcessStatuses();
            }
        }
    }
}