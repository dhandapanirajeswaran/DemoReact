using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
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

        public IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? forDate=null)
        {
            return _db.GetSitesWithEmailsAndPrices(forDate);
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
        {
            return _db.GetSitesWithPrices(forDate, siteId, pageNo, pageSize);
        }

        public Site GetSite(int id)
        {
            return _db.GetSite(id);
        }

        public Site NewSite(Site site)
        {
            return _db.NewSite(site);
        }

        public SitePriceViewModel GetSiteAndPrices(int siteId, DateTime date)
        {
            return _db.GetASiteWithPrices(siteId, date);
        }

        public bool ExistsSite(string siteName, int? catNo)
        {
            return _db.GetSites().Any(m => m.SiteName.Equals(siteName, StringComparison.CurrentCultureIgnoreCase) || 
                (catNo.HasValue && m.CatNo.HasValue && m.CatNo.Value == catNo.Value));
        }

        public bool UpdateSite(Site site)
        {
           return _db.UpdateSite(site);
        }

        public IEnumerable<SiteToCompetitor> GetCompetitors(int siteId, int driveTimeFrom, int driveTimeTo, bool includeSainsburysAsCompetitors = true)
        {
            var competitors = _db.GetCompetitors(siteId, driveTimeFrom, driveTimeTo);
            return competitors.ToList();
        }
    }
}