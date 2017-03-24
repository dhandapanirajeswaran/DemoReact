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
	public class SiteService : ISiteService
	{
		protected readonly IPetrolPricingRepository _db;

		public SiteService(IPetrolPricingRepository db)
		{
			_db = db;
		}

		public IEnumerable<Site> GetJsSites()
		{
			return _db.GetJsSites().ToList().Take(Constants.SitesPageSize);
		}

		public IEnumerable<Site> GetSites()
		{
			return _db.GetSites().ToList();
		}

		public Dictionary<string, int> GetCompanies()
		{
			return _db.GetCompanies();
		}

		public IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? forDate = null)
		{
			return _db.GetSitesWithEmailsAndPrices(forDate);
		}

		public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, string storeName = "", int catNo = 0, int storeNo = 0, string storeTown = "", int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
		{
			return _db.GetSitesWithPrices(forDate, storeName, catNo, storeNo, storeTown, siteId, pageNo, pageSize);
		}

		public IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
		{
			return _db.GetCompetitorsWithPrices(forDate, siteId, pageNo, pageSize);
		}

		public Site GetSite(int id)
		{
			return _db.GetSite(id);
		}

		public Site NewSite(Site site)
		{
			return _db.NewSite(site);
		}

		public SitePriceViewModel GetSiteAndPrices(int siteId, DateTime date, string storeName)
		{
			return _db.GetASiteWithPrices(siteId, date, storeName);
		}

		public bool ExistsSite(Site site)
		{
			return _db.GetSites().Any(m =>
				m.SiteName.Equals(site.SiteName, StringComparison.CurrentCultureIgnoreCase)
				|| (site.CatNo.HasValue && m.CatNo.HasValue && m.CatNo.Value == site.CatNo.Value));
		}

		public bool IsUnique(Site site)
		{
			if(site.Id > 0)
				return false == _db.GetSites().Any(m =>
				(m.SiteName.Equals(site.SiteName, StringComparison.CurrentCultureIgnoreCase)
				|| (site.StoreNo.HasValue && m.StoreNo.HasValue && m.StoreNo.Value == site.StoreNo.Value)
				|| (site.PfsNo.HasValue && m.PfsNo.HasValue && m.PfsNo.Value == site.PfsNo.Value)
				) && m.Id != site.Id);
			else
				return false == _db.GetSites().Any(m =>
				m.SiteName.Equals(site.SiteName, StringComparison.CurrentCultureIgnoreCase)
				|| (site.StoreNo.HasValue && m.StoreNo.HasValue && m.StoreNo.Value == site.StoreNo.Value)
				|| (site.PfsNo.HasValue && m.PfsNo.HasValue && m.PfsNo.Value == site.PfsNo.Value));
		}

        public bool HasDuplicateEmailAddresses(Site site)
        {
            return site.Emails.Any(x => site.Emails.Count(e => String.Compare(e.EmailAddress, x.EmailAddress, true) == 0) > 1);
        }

		public bool UpdateSite(Site site)
		{
			return _db.UpdateSite(site);
		}

        public SiteToCompetitor GetCompetitor(int siteId, int competitorId)
        {
            return _db.GetCompetitor(siteId, competitorId);
        }


        public void UpdateSiteToCompetitor(List<SiteToCompetitor> newSiteToCompetitorRecords)
        {
             _db.UpdateSiteToCompetitor(newSiteToCompetitorRecords);
        }

        public bool RemoveExcludeBrand(string strBrandName)
        {
            return _db.RemoveExcludeBrand(strBrandName);
        }

        public bool SaveExcludeBrands(List<String> listOfBrands)
        {
            return _db.SaveExcludeBrands(listOfBrands);
        }

        public List<String> GetExcludeBrands()
        {
            return _db.GetExcludeBrands();
        }

        public SiteNoteViewModel GetSiteNote(int siteId)
        {
            return _db.GetSiteNote(siteId);
        }

        public JsonResultViewModel<bool> UpdateSiteNote(SiteNoteUpdateViewModel model)
        {
            return _db.UpdateSiteNote(model);
        }

        public JsonResultViewModel<int> DeleteSiteNote(int siteId)
        {
            return _db.DeleteSiteNote(siteId);
        }

        public RecentFileUploadSummary GetRecentFileUploadSummary()
        {
            return _db.GetRecentFileUploadSummary();
        }

        public IEnumerable<ContactDetail> GetContactDetails()
        {
            return _db.GetContactDetails();
        }
    }
}