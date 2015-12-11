using System;
using System.Collections.Generic;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IPetrolPricingRepository
    {
        
        IEnumerable<Site> GetSites();
        
        Site GetSite(int siteId);

        SitePriceViewModel GetASiteWithPrices(int siteId, DateTime forDate);

        Site GetSiteByCatNo(int catNo);

        Site NewSite(Site site);

        bool UpdateSite(Site site);

        bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber);

        IEnumerable<SiteToCompetitor> GetCompetitors(int siteId, int driveTimeFrom, int driveTimeTo, bool includeSainsburysAsCompetitors = true);

        IEnumerable<Site> GetSitesWithPricesAndCompetitors();

        IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize);

        IQueryable<Site> GetSitesIncludePrices(DateTime? forDate = null);

        IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId, DateTime usingPricesForDate);

        void UpdateImportProcessStatus(FileUpload fileUpload, int statusId);

        IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadType, int? statusId);

        FileUpload GetFileUpload(int id);

        FileUpload NewUpload(FileUpload upload);
        
        bool ExistsUpload(string storedFileName);

        bool AnyFileUploadForDate(DateTime date, UploadType uploadType);

        void LogImportError(FileUpload fileDetails, string errorMessage, int? lineNumber);

        void Dispose();

        SitePrice AddOrUpdateSitePriceRecord(SitePrice calculatedSitePrice);

        // Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
        void DeleteRecordsForOlderImportsOfDate(DateTime ofDate, int uploadId);
    }

    public interface IPetrolPricingRepositoryLookup
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();

        IEnumerable<AppConfigSettings> GetAppConfigSettings();
    }
}
