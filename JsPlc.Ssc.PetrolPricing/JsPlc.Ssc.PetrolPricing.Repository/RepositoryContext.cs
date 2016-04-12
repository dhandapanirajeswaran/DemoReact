using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Common;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Persistence;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryContext: DbContext, IRepositoryContext
    {
        public IDbSet<FuelType> FuelType { get; set; } // 1=Super, 2=Unleaded,  6=Std Dis, // Unused 5=Super Dis, 7=LPG
        public IDbSet<UploadType> UploadType { get; set; } // Daily, Quarterly
        public IDbSet<ImportProcessStatus> ImportProcessStatus { get; set; } // Uploaded,Processing,Success,Failed

        public IDbSet<Site> Sites { get; set; }
        public IDbSet<SiteEmail> SiteEmails { get; set; }
        public IDbSet<SitePrice> SitePrices { get; set; }

        public IDbSet<SiteToCompetitor> SiteToCompetitors { get; set; }
        public IDbSet<FileUpload> FileUploads { get; set; }

        public IDbSet<DailyUploadStaging> DailyUploadStaging { get; set; }
        public IDbSet<QuarterlyUploadStaging> QuarterlyUploadStaging { get; set; }
        public IDbSet<ImportProcessError> ImportProcessErrors { get; set; }

        public IDbSet<EmailSendLog> EmailSendLogs { get; set; }
        
        public IDbSet<DailyPrice> DailyPrices { get; set; }

        public RepositoryContext() : base("name=PetrolPricingRepository") {
			Database.SetInitializer<RepositoryContext>(null);
		}
    }
}
