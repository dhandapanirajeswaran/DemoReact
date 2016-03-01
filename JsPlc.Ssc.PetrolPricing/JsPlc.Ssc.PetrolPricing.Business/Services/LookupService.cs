using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class LookupService : ILookupService
    {
        private readonly IPetrolPricingRepository _db;

        public LookupService(IPetrolPricingRepository db)
        {
            _db = db;
        }

        public IEnumerable<UploadType> GetUploadTypes()
        {
            return _db.GetUploadTypes();
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            return _db.GetFuelTypes();
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            return _db.GetProcessStatuses();
        }
    }
}