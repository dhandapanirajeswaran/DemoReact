using System.Data.Entity;
using JsPlc.Ssc.PetrolPricing.Models;


namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IRepositoryContext
    {
        IDbSet<FuelType> FuelType { get; } // 1=Super, 2=Unleaded, 5=Super Dis, 6=Std Dis, 7=LPG
        IDbSet<UploadType> UploadType { get; } // Daily, Quarterly
        IDbSet<ImportProcessStatus> ImportProcessStatus { get; } // Uploaded,Processing,Success,Failed

        IDbSet<Site> Sites { get; }
        IDbSet<SiteToCompetitor> SiteToCompetitors { get; }
        IDbSet<FileUpload> FileUploads { get; }
        IDbSet<DailyUploadStaging> DailyUploadStaging { get; }
        IDbSet<QuarterlyUploadStaging> QuarterlyUploadStaging { get; }
    }
}
