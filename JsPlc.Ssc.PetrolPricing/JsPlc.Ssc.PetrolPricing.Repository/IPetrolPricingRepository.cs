using System;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IPetrolPricingRepository
    {
        
        IEnumerable<Site> GetSites();
        
        Site GetSite(int siteId);

        Site NewSite(Site site);

        void UpdateSite(Site site);

        IEnumerable<FileUpload> GetFileUploads(DateTime? date, UploadType uploadType);

        FileUpload NewUpload(FileUpload upload);
        
        bool ExistsUpload(string storedFileName);

        bool AnyFileUploadForDate(DateTime date, UploadType uploadType);

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
