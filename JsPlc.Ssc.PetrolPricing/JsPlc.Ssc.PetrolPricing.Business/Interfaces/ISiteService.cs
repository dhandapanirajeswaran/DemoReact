using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ISiteService
    {
        IEnumerable<Site> GetJsSites();

        IEnumerable<Site> GetSites();

        Dictionary<string, int> GetCompanies();

        IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? forDate = null);

        IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, string storeName = "", int catNo = 0, int storeNo = 0, string storeTown = "", int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize);

        IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize, string siteIds = null);

        Site GetSite(int id);

        Site NewSite(Site site);

        SitePriceViewModel GetSiteAndPrices(int siteId, DateTime date, string storeName);

		bool ExistsSite(Site site);

        bool UpdateSite(Site site);

        bool HasDuplicateEmailAddresses(Site site);

		bool IsUnique(Site site);

        SiteToCompetitor GetCompetitor(int siteId, int competitorId);

        void UpdateSiteToCompetitor(List<SiteToCompetitor> newSiteToCompetitorRecords);

        bool RemoveExcludeBrand(string strBrandName);

        bool SaveExcludeBrands(List<String> listOfBrands);

        List<String> GetExcludeBrands();

        SiteNoteViewModel GetSiteNote(int siteId);
        JsonResultViewModel<bool> UpdateSiteNote(SiteNoteUpdateViewModel model);

        JsonResultViewModel<int> DeleteSiteNote(int siteId);

        RecentFileUploadSummary GetRecentFileUploadSummary();

        IEnumerable<ContactDetail> GetContactDetails();

        IEnumerable<int> GetJsSitesByPfsNum();

        StatusViewModel RemoveAllSiteEmailAddresses();

        IEnumerable<SiteEmailAddressViewModel> GetAllSiteEmailAddresses(int siteId = 0);
        StatusViewModel UpsertSiteEmailAddresses(IEnumerable<SiteEmailImportViewModel> emailAddresses);

        void RebuildSiteAttributes();

        SitePriceViewModel GetTodayPricesForCalcPrice(DateTime forDate, int siteId);

        IEnumerable<NearbySiteViewModel> GetNearbyCompetitorSites(int siteId);

        SiteEmailTodaySendStatusViewModel GetSiteEmailTodaySendStatuses(DateTime forDate);
        JsPriceOverrideViewModel GetJsPriceOverrides(int fileUploadId);
    }
}
