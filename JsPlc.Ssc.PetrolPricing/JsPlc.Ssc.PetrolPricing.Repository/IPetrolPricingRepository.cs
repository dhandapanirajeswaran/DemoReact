﻿using System;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IPetrolPricingRepository
    {
        
        IEnumerable<Site> GetSites();
        
        Site GetSite(int siteId);

        Site GetSiteByCatNo(int catNo);

        Site NewSite(Site site);

        bool UpdateSite(Site site);

        bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber);

        IEnumerable<SiteToCompetitor> GetCompetitors(int siteId, int driveTimeFrom, int driveTimeTo, bool includeSainsburysAsCompetitors = true);

        IEnumerable<Site> GetSitesWithPricesAndCompetitors();

        IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId, DateTime usingUploadDate);

        void UpdateImportProcessStatus(FileUpload importProcessStatus, int StatusId);

        IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadType, int? statusId);

        FileUpload GetFileUpload(int id);

        FileUpload NewUpload(FileUpload upload);
        
        bool ExistsUpload(string storedFileName);

        bool AnyFileUploadForDate(DateTime date, UploadType uploadType);

        void LogImportError(FileUpload fileDetails, string errorMessage, int? lineNumber);

        void Dispose();

    }

    public interface IPetrolPricingRepositoryLookup
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();

        IEnumerable<AppConfigSettings> GetAppConfigSettings();
    }
}
