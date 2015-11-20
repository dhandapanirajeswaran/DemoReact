using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Common;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryContext:DbContext,IRepositoryContext
    {
        public IDbSet<FuelType> FuelType { get; set; } // 1=Super, 2=Unleaded, 5=Super Dis, 6=Std Dis, 7=LPG
        public IDbSet<UploadType> UploadType { get; set; } // Daily, Quarterly
        public IDbSet<ImportProcessStatus> ImportProcessStatus { get; set; } // Uploaded,Processing,Success,Failed

        public IDbSet<Site> Sites { get; set; }
        public IDbSet<SiteToCompetitor> SiteToCompetitors { get; set; }
        public IDbSet<FileUpload> FileUploads { get; set; }
        public IDbSet<DailyUploadStaging> DailyUploadStaging { get; set; }
        public IDbSet<QuarterlyUploadStaging> QuarterlyUploadStaging { get; set; }

        public RepositoryContext() : base("name=PetrolPricingRepository") { }

        public RepositoryContext(DbConnection connection) : base(connection, true) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Site).WithMany().HasForeignKey(m => m.SiteId);
            modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Competitor).WithMany().HasForeignKey(m => m.CompetitorId);
        }
    }
}
