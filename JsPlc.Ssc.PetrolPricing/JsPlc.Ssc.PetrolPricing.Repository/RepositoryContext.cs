using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Common;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Persistence;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Repository.Dapper;
using System;
using Dapper;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.Enums;

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

        public IDbSet<SystemSettings> SystemSettings { get; set; }

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
            const string sproc = "spCalculateSitePricesForDate";

            var parameters = new
            {
                @ForDate = forDate,
                @SiteIds = siteIds
            };

            return DapperHelper.QueryList<FuelPriceViewModel>(this, sproc, parameters);
        }

        public RecentFileUploadSummary GetRecentFileUploadSummary()
        {
            const string sproc = "spGetRecentFileUploadSummary";

            var parameters = new { };

            return new RecentFileUploadSummary()
            {
                Files = DapperHelper.QueryList<RecentFileUploadSummaryItem>(this, sproc, parameters)
            };
        }

        public IEnumerable<SitePriceViewModel> GetCompetitorsWithPriceView(DateTime forDate, int siteId)
        {
            const string sproc = "GetCompetitorsWithPriceView";

            var parameters = new
            {
                @ForDate = forDate,
                @SiteId = siteId
            };

            var model = DapperHelper.QueryMultiple<List<SitePriceViewModel>>(this, sproc, parameters, FillGetCompetitorsWithPriceView);
            return model;
        }

        private void FillGetCompetitorsWithPriceView(List<SitePriceViewModel> model, SqlMapper.GridReader multiReader)
        {
            var compsites = multiReader.Read<SitePriceViewModel>().AsList();
            if (compsites != null)
            {
                model.AddRange(compsites);
                var prices = multiReader.Read<FuelPriceViewModel>();
                foreach(var price in prices)
                {
                    var compsite = compsites.FirstOrDefault(x => x.SiteId == price.SiteId);
                    if (compsite != null)
                    {
                        if (compsite.FuelPrices == null)
                            compsite.FuelPrices = new List<FuelPriceViewModel>();

                        compsite.FuelPrices.Add(price);
                    }
                }
            }
        }


        public IEnumerable<ContactDetail> GetContactDetails()
        {
            const string sproc = "spGetContactDetailsList";

            var parameters = new
            {
            };

            var model = DapperHelper.QueryList<ContactDetail>(this, sproc, parameters);
            return model;
        }

        public PPUserPermissions GetUserPermissions(int ppUserId)
        {
            const string sproc = "spGetUserPermissions";

            var parameters = new
            {
                @PPUserId = ppUserId
            };

            var model = DapperHelper.QueryFirst<PPUserPermissions>(this, sproc, parameters);
            return model;
        }

        public bool UpserUserPermissions(int requestingPPUserId, PPUserPermissions permissions)
        {
            const string sproc = "spUpsertUserPermissions";

            var parameters = new
            {
                @PPUserId = permissions.PPUserId,
                @IsAdmin = permissions.IsAdmin,
                @FileUploadsUserPermissions = permissions.FileUploadsUserPermissions,
                @SitePricingUserPermissions = permissions.SitePricingUserPermissions,
                @SitesMaintenanceUserPermissions = permissions.SitesMaintenanceUserPermissions,
                @ReportsUserPermissions = permissions.ReportsUserPermissions,
                @UsersManagementUserPermissions = permissions.UsersManagementUserPermissions,
                @DiagnosticsUserPermissions = permissions.DiagnosticsUserPermissions,
                @RequestingPPUserId = requestingPPUserId
            };

            var result = DapperHelper.QueryScalar(this, sproc, parameters);
            return result == 0;
        }

        public IEnumerable<NearbyGrocerPriceSiteStatus> GetNearbyGrocerPriceStatusForSites(DateTime forDate, string siteIds, int driveTime)
        {
            const string sproc = "spNearbyGrocerPriceStatusForSites";

            DiagnosticLog.StartDebug("sproc: " + sproc);

            var parameters = new
            {
                @ForDate = forDate,
                @driveTime = driveTime,
                @SiteIds = siteIds
            };
            var model = DapperHelper.QueryList<NearbyGrocerPriceSiteStatus>(this, sproc, parameters);

            DiagnosticLog.EndDebug("sproc: " + sproc);
            return model;
        }

        public IEnumerable<DiagnosticsDatabaseObject> GetDiagnosticsRecentDatabaseObjectChanges(int daysAgo)
        {
            const string sproc = "spGetDiagnosticsRecentDatabaseObjectChanges";

            var parameters = new
            {
                @DaysAgo = daysAgo
            };
            var model = DapperHelper.QueryList<DiagnosticsDatabaseObject>(this, sproc, parameters, disableDapperLog: true);
            return model;
        }

        public DiagnosticsDatabaseObjectSummary GetDiagnosticsDatabaseObjectSummary()
        {
            const string sproc = "spGetDiagnosticsDatabaseObjectSummary";

            var model = DapperHelper.QueryFirst<DiagnosticsDatabaseObjectSummary>(this, sproc, null, disableDapperLog: true);
            return model;
        }

        public void CreateDefaultUserPermissionsForNewUser(int ppUserId, int requestingPPUserId)
        {
            if (ppUserId == 0)
                throw new ArgumentException("PPUserId cannot be zero");

            const string sprocName = "spUpsertUserPermissions";

            var parameters = new
            {
                @PPUserId = ppUserId,
                @IsAdmin = 0,
                @FileUploadsUserPermissions = FileUploadsUserPermissions.NewUserDefaults,
                @SitePricingUserPermissions = SitesPricingUserPermissions.NewUserDefaults,
                @SitesMaintenanceUserPermissions = SitesMaintenanceUserPermissions.NewUserDefaults,
                @ReportsUserPermissions = ReportsUserPermissions.NewUserDefaults,
                @UsersManagementUserPermissions = UsersManagementUserPermissions.NewUserDefaults,
                @DiagnosticsUserPermissions = DiagnosticsUserPermissions.NewUserDefaults,
                @RequestingPPUserId = requestingPPUserId
            };

            DapperHelper.Execute(this, sprocName, parameters);
        }

        public void DeleteUserPermissions(int ppUserId)
        {
            if (ppUserId == 0)
                throw new ArgumentException("PPUserId cannot be zero");

            const string sprocName = "spDeleteUserPermissions";

            var parameters = new
            {
                @PPUserId = ppUserId
            };

            DapperHelper.Execute(this, sprocName, parameters);
        }

        public void ArchiveQuarterlyUploadStagingData()
        {
            const string sprocName = "spArchiveQuarterlyUploadData";

            var parameters = new { };

            DapperHelper.Execute(this, sprocName, parameters);
        }

        public IEnumerable<SelectItemViewModel> GetQuarterlyFileUploadOptions()
        {
            const string sprocName = "spGetQuarterlyFileUploadOptions";

            var parameters = new { };

            return DapperHelper.QueryList<SelectItemViewModel>(this, sprocName, parameters);
        }

        public IEnumerable<QuarterlySiteAnalysisReportRowViewModel> GetQuarterlySiteAnalysisReportRows(int leftFileUploadId, int rightFileUploadId)
        {
            const string sprocName = "spGetQuarterlySiteAnalysisReport";

            var parameters = new
            {
                @LeftFileUploadId = leftFileUploadId,
                @RightFileUploadId = rightFileUploadId
            };

            return DapperHelper.QueryList<QuarterlySiteAnalysisReportRowViewModel>(this, sprocName, parameters);
        }
    }
}
