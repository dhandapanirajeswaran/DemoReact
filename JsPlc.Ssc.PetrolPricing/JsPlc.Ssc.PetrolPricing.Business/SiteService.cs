using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public IEnumerable<Site> GetSitesWithPricesAndCompetitors()
        {
            return _db.GetSitesWithPricesAndCompetitors();
        }

        public Site GetSite(int id)
        {
            return _db.GetSite(id);
        }

        public Site NewSite(Site site)
        {
            return _db.NewSite(site);
        }

        public bool ExistsSite(string siteName, int? catNo)
        {
            return _db.GetSites().Any(m => m.SiteName.Equals(siteName, StringComparison.CurrentCultureIgnoreCase) || 
                (catNo.HasValue && m.CatNo.HasValue && m.CatNo.Value == catNo.Value));
        }

        public IEnumerable<Site> GetCompetitors(int siteId, int distFrom, int distTo, bool includeSainsburysAsCompetitors = true)
        public bool UpdateSite(Site site)
        {
           return _db.UpdateSite(site);
        }

        public bool ExistsSite(string siteName)
        {
            var competitors = _db.GetCompetitors(siteId, distFrom, distTo);
            return competitors.ToList();
        }
    }
}