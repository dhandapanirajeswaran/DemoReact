using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class SiteService : BaseService
    {
        public SiteService() { }

        public SiteService(IPetrolPricingRepository repository) : base(repository) { }

        public IEnumerable<Site> GetSites()
        {
           return _db.GetSites().ToList();
        }

        public Site GetSite(int id)
        {
            return _db.GetSite(id);
        }

        public Site NewSite(Site site)
        {
            return _db.NewSite(site);
        }

        public bool UpdateSite(Site site)
        {
           return _db.UpdateSite(site);
        }

        public bool ExistsSite(string siteName)
        {
            return _db.GetSites().Any(s => s.SiteName.Equals(siteName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}