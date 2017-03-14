using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Common;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Persistence;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Repository.Dapper;
using System;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
	public class RepositoryContext : DbContext, IRepositoryContext
	{
		public IDbSet<FuelType> FuelType { get; set; } // 1=Super, 2=Unleaded,  6=Std Dis, // Unused 5=Super Dis, 7=LPG
		public IDbSet<UploadType> UploadType { get; set; } // Daily, Quarterly
		public IDbSet<ImportProcessStatus> ImportProcessStatus { get; set; } // Uploaded,Processing,Success,Failed

        public IDbSet<PPUser> PPUsers { get; set;  }

		public IDbSet<Site> Sites { get; set; }
		public IDbSet<SiteEmail> SiteEmails { get; set; }
		public IDbSet<SitePrice> SitePrices { get; set; }

		public IDbSet<SiteToCompetitor> SiteToCompetitors { get; set; }
		public IDbSet<FileUpload> FileUploads { get; set; }

		public IDbSet<DailyUploadStaging> DailyUploadStaging { get; set; }
		public IDbSet<QuarterlyUploadStaging> QuarterlyUploadStaging { get; set; }
        public IDbSet<LatestPrice> LatestPrices { get; set; }
        public IDbSet<LatestCompPrice> LatestCompPrices { get; set; }
		public IDbSet<ImportProcessError> ImportProcessErrors { get; set; }

		public IDbSet<EmailSendLog> EmailSendLogs { get; set; }

		public IDbSet<DailyPrice> DailyPrices { get; set; }

        public IDbSet<ExcludeBrands> ExcludeBrands { get; set; }

		public RepositoryContext()
			: base("name=PetrolPricingRepository")
		{
			Database.SetInitializer<RepositoryContext>(null);
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Configuration.LazyLoadingEnabled = true; // we want to control loading of child entities
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

			modelBuilder.Entity<Site>().HasMany(m => m.Competitors).WithRequired(x => x.Site).HasForeignKey(x => x.SiteId);

			modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Site).WithMany().HasForeignKey(m => m.SiteId);
			modelBuilder.Entity<SiteToCompetitor>().HasRequired(m => m.Competitor).WithMany().HasForeignKey(m => m.CompetitorId);

			modelBuilder.Entity<Site>().HasMany(m => m.Emails).WithRequired(x => x.Site).HasForeignKey(x => x.SiteId);
			modelBuilder.Entity<Site>().HasMany(m => m.Prices).WithRequired(x => x.JsSite).HasForeignKey(x => x.SiteId);
			//modelBuilder.Entity<SiteEmail>().HasRequired(c => c.Site).WithMany(x => x.Emails).HasForeignKey(m => m.SiteId);

			modelBuilder.Entity<DailyPrice>().HasRequired(m => m.FuelType).WithMany().HasForeignKey(x => x.FuelTypeId);
            modelBuilder.Entity<LatestPrice>().HasRequired(m => m.DailyUpload).WithMany().HasForeignKey(x => x.FuelTypeId);
            modelBuilder.Entity<LatestCompPrice>().HasRequired(m => m.DailyUpload).WithMany().HasForeignKey(x => x.FuelTypeId); 

			modelBuilder.Entity<FileUpload>().HasRequired(x => x.Status).WithMany().HasForeignKey(y => y.StatusId);
            modelBuilder.Entity<ExcludeBrands>();
             
			modelBuilder.Entity<FileUpload>()
				.HasMany(x => x.ImportProcessErrors)
				.WithRequired(x => x.Upload)
				.HasForeignKey(x => x.UploadId);
		}

        public List<FuelPriceViewModel> CalculateFuelPricesForSitesAndDate(DateTime forDate, string siteIds)
        {
            var sproc = "spCalculateSitePricesForDate";

            var parameters = new
            {
                @ForDate = forDate,
                @SiteIds = siteIds
            };

            return DapperHelper.QueryList<FuelPriceViewModel>(this, sproc, parameters);
        }
    }
}
