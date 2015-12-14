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
      
        /// <summary>
        /// Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
        /// Clear criteria = Where date = today and fileId <> the successful Id (afile.Id)
        /// </summary>
        /// <param name="ofDate"></param>
        /// <param name="uploadId"></param>
        void DeleteRecordsForOlderImportsOfDate(DateTime ofDate, int uploadId);

        /// <summary>
        /// Do we have any daily prices for a given fuelId on the date
        /// </summary>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesforDate"></param>
        /// <returns></returns>
        bool AnyDailyPricesForFuelOnDate(int fuelId, DateTime usingPricesforDate);


        /// <summary>
        /// Gets the FileUpload available for Calc/ReCalc 
        /// i.e those which has been imported to DailyPrice either Successfully (or CalcFailed previously to allow rerun)
        /// </summary>
        /// <param name="forDate"></param>
        /// <returns>Returns null if none available</returns>
        FileUpload GetDailyFileAvailableForCalc(DateTime forDate);

        /// <summary>
        /// Any file in status Calculating
        /// </summary>
        /// <param name="forDate">Date concerned</param>
        /// <returns>FileUpload</returns>
        FileUpload GetDailyFileWithCalcRunningForDate(DateTime forDate);
    }

    public interface IPetrolPricingRepositoryLookup
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();

        IEnumerable<AppConfigSettings> GetAppConfigSettings();
    }
}
