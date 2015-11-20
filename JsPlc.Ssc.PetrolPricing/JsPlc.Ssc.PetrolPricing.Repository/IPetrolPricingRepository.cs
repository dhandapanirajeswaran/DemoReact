using System;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IPetrolPricingRepository
    {
        
        IEnumerable<Site> GetSites();
        
        Site GetSite(int siteId);
        
        IEnumerable<FileUpload> GetFileUploads(DateTime? date, UploadType uploadType);

        bool AnyFileUploadForDate(DateTime date, UploadType uploadType);

        void Dispose();
    }

    public interface IPetrolPricingRepositoryLookup
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();
    }
}
