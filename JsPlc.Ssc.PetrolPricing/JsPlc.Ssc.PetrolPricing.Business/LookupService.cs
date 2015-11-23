using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class LookupService
    {
        public static IEnumerable<UploadType> GetUploadTypes()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetUploadTypes();
            }
        }

        public static IEnumerable<FuelType> GetFuelTypes()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetFuelTypes();
            }
        }

        public static IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetProcessStatuses();
            }
        }
    }
}