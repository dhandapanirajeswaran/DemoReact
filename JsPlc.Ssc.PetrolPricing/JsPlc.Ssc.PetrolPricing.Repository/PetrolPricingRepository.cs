using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository:IPetrolPricingRepository
    {
        private readonly RepositoryContext _db;

        public PetrolPricingRepository() { }

        public PetrolPricingRepository(RepositoryContext context) { _db = context; }

        public IEnumerable<Site> GetSites()
        {
           return _db.Sites.OrderBy(q=>q.Id);
        }

        public Site GetSite(int id)
        {
            return _db.Sites.FirstOrDefault(q => q.Id == id);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        
    }
}
