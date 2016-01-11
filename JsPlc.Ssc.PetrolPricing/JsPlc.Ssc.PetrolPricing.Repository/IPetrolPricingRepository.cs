﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IPetrolPricingRepository
    {
        
        IEnumerable<Site> GetSites();

        IEnumerable<Site> GetJsSites();
        
        Site GetSite(int siteId);

        SitePriceViewModel GetASiteWithPrices(int siteId, DateTime forDate);

        Site GetSiteByCatNo(int catNo);

        Site NewSite(Site site);

        bool UpdateSite(Site site);

        bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber);

        bool NewQuarterlyRecords(List<CatalistQuarterly> siteCatalistData, FileUpload fileDetails,
            int startingLineNumber);

        // RUN sprocs to Add/Update/Delete sites and siteToCompetitors
        Task<bool> ImportQuarterlyUploadStaging(int uploadId);

        /// <summary>
        /// Useful for SiteMaint screen
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveTimeFrom"></param>
        /// <param name="driveTimeTo"></param>
        /// <param name="includeSainsburysAsCompetitors"></param>
        /// <returns></returns>
        IEnumerable<SiteToCompetitor> GetCompetitors(int siteId, int driveTimeFrom, int driveTimeTo, bool includeSainsburysAsCompetitors = true);

        SiteToCompetitor LookupSiteAndCompetitor(int siteCatNo, int competitorCatNo);
        
        //IEnumerable<Site> GetSitesWithPricesAndCompetitors();
        /// <summary>
        /// Useful for emailing
        /// </summary>
        /// <param name="fromPriceDate">optional</param>
        /// <param name="toPriceDate">optional</param>
        /// <returns></returns>
        IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? fromPriceDate = null, DateTime? toPriceDate = null);
        
        IEnumerable<Site> GetSitesWithCompetitors();

        /// <summary>
        /// Useful for pricing screen
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="siteId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize);

        /// <summary>
        /// Useful for pricing screen
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="siteId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize);

        // unsafe to use , see impl.
        IQueryable<Site> GetSitesIncludePrices(DateTime? forDate = null);

        IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId, DateTime usingPricesForDate);

        void UpdateImportProcessStatus(int statusId, FileUpload fileUpload);

        Task<List<FileUpload>> GetFileUploads(DateTime? date, int? uploadType, int? statusId);

        FileUpload GetFileUpload(int id);

        FileUpload NewUpload(FileUpload upload);
        
        bool ExistsUpload(string storedFileName);

        Task<bool> AnyFileUploadForDate(DateTime date, UploadType uploadType);

        void LogImportError(FileUpload fileDetails, string errorMessage, int? lineNumber);

        /// <summary>
        /// Logs all entries in list to EmailSendLog table in one go.
        /// </summary>
        /// <param name="logItems"></param>
        /// <returns></returns>
        Task<List<EmailSendLog>> LogEmailSendLog(List<EmailSendLog> logItems);

        /// <summary>
        /// Mark file status = Failed for any imports/calcs exceeding 1min, 5 min respectively
        /// </summary>
        void FailHangedFileUploadOrCalcs(int importTimeoutMilliSec, int calcTimeoutMilliSec);

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
        /// Delete all QuarterlyUploadStaging records prior to starting Import of QuarterlyUploadStaging
        /// </summary>
        Task<bool> DeleteRecordsForQuarterlyUploadStaging();

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

        //bool UpdateCatalistQuarterlyData(List<CatalistQuarterly> CatalistQuarterlyData, FileUpload fileDetails, bool isSainsburys);

        Task<int> CreateMissingSuperUnleadedFromUnleaded(DateTime forDate, int markup, int siteId = 0);

        /// <summary>
        /// Only updates OverridePrice value
        /// </summary>
        /// <param name="prices"></param>
        /// <param name="forDate"></param>
        Task<int> SaveOverridePricesAsync(List<SitePrice> prices, DateTime? forDate = null);

        /// <summary>
        /// Gets the details of the competitor sites within 0-5mins, 5-10mins, 15-20mins & 20-25mins from Sainsbury’s. 
        /// Split by Tesco, Morrison’s Asda, BP etc.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        CompetitorSiteReportViewModel GeReporttCompetitorSite(int siteId);

        /// <summary>
        /// Gets the details of the number of sites at each price point, split by brand
        /// </summary>
        /// <param name="when">The date to compare price points on</param>
        /// <param name="fuelTypeId">The typeof of fuel to compare on</param>
        /// <returns></returns>
        PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId);

        /// <summary>
        /// Gets the national average report for the specified date
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        NationalAverageReportViewModel GetReportNationalAverage(DateTime when);

        Task<List<EmailSendLog>> GetEmailSendLog(int siteId, DateTime forDate);
    }

    public interface IPetrolPricingRepositoryLookup
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();

        IEnumerable<AppConfigSettings> GetAppConfigSettings();
    }
}
