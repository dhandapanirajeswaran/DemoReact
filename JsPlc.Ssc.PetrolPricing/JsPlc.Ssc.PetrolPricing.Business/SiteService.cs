using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class SiteService : IDisposable
    {
        public void Dispose()
        {
            // do nothing for now
        }

        public IEnumerable<Site> GetSites()
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetSites().ToList();
            }
        }

        public Site GetSite(int id)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                return db.GetSite(id);
            }
        }
    }
}