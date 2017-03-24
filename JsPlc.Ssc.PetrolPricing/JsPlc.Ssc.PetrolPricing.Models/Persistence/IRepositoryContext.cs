using System.Data.Entity;
using JsPlc.Ssc.PetrolPricing.Models;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.Persistence
{
    public interface IRepositoryContext
    {
        IDbSet<FuelType> FuelType { get; } // 1=Super, 2=Unleaded, 6=Std Dis (only 3 main) // 5=Super Dis, 7=LPG
        IDbSet<UploadType> UploadType { get; } // Daily, Quarterly
        IDbSet<ImportProcessStatus> ImportProcessStatus { get; } // Uploaded,Processing,Success,Failed

        IDbSet<PPUser> PPUsers { get; set; }

        IDbSet<Site> Sites { get; }
        IDbSet<SiteEmail> SiteEmails { get; set; }
        IDbSet<SitePrice> SitePrices { get; set; }

        IDbSet<SiteToCompetitor> SiteToCompetitors { get; }
        IDbSet<FileUpload> FileUploads { get; }
        IDbSet<ImportProcessError> ImportProcessErrors { get; set; }

        IDbSet<DailyUploadStaging> DailyUploadStaging { get; }
        IDbSet<QuarterlyUploadStaging> QuarterlyUploadStaging { get; }

        IDbSet<DailyPrice> DailyPrices { get; set; }
        IEnumerable<ContactDetail> GetContactDetails();
    }
}
