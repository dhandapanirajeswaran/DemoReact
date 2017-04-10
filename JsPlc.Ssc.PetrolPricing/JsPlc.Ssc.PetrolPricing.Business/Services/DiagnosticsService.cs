using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class DiagnosticsService : IDiagnosticsService
    {
        protected readonly IPetrolPricingRepository _db;
        protected readonly ILookupService _lookupService;
        protected readonly IFactory _factory;
        protected readonly IAppSettings _appSettings;
        protected readonly ILogger _logger;

        public DiagnosticsService(IPetrolPricingRepository db,
            IAppSettings appSettings,
            ILookupService lookupSerivce,
            IFactory factory
            )
        {
            _db = db;
            _lookupService = lookupSerivce;
            _factory = factory;
            _appSettings = appSettings;
            _logger = new PetrolPricingLogger();
        }

        #region implementation of IDiagnosticsService

        public DiagnosticsViewModel GetDiagnostics(int daysAgo)
        {
            var model = new DiagnosticsViewModel()
            {
            };

            var diagnosticsLog = DiagnosticLog.CloneLogEntries().OrderBy(x => x.Created);
            foreach(var log in diagnosticsLog)
            {
                model.LogEntries.Add(new DiagnosticsLogEntryViewModel()
                {
                    Created = log.Created,
                    Level = log.Level,
                    Message = log.Message,
                    Exception = log.Exception,
                    Parameters = log.Parameters
                });
            }

            var currentThead = System.Threading.Thread.CurrentThread;

            var environment = new Dictionary<string, object>()
            {
                {"DateAndTime.Now", DateTime.Now },
                {"DateAndTime.UtcNow", DateTime.UtcNow },
                {"CurrentThread.CurrentCulture.EnglishName", currentThead.CurrentCulture.EnglishName },
                {"CurrentThread.CurrentCulture.DateTimeFormat", currentThead.CurrentCulture.DateTimeFormat.LongDatePattern },
                {"CurrentThread.CurrentCulture.DisplayName", currentThead.CurrentCulture.DisplayName }
            };

            var appSettings = new Dictionary<string, object>()
            {
                {"appSetting.ExcelFileSheetName", _appSettings.ExcelFileSheetName },
                {"appSetting.EmailSubject", _appSettings.EmailSubject },
                {"appSetting.EmailFrom", _appSettings.EmailFrom },
                {"appSetting.FixedEmailTo", _appSettings.FixedEmailTo },
                {"appSetting.SuperUnleadedMarkup", _appSettings.SuperUnleadedMarkup },
            };

            var coreSettings = new Dictionary<string, object>()
            {
                {"Dapper.LogDatabaseCalls", CoreSettings.RepositorySettings.Dapper.LogDapperCalls },

                {"SitePrices.UseStoredProcedure", CoreSettings.RepositorySettings.SitePrices.UseStoredProcedure },
                {"SitePrices.ShouldCompareWithOldCode", CoreSettings.RepositorySettings.SitePrices.ShouldCompareWithOldCode },
                {"SitePrices.CompareOutputFilename", CoreSettings.RepositorySettings.SitePrices.CompareOutputFilename},

                {"CompetitorPrices.UseStoredProcedure", CoreSettings.RepositorySettings.CompetitorPrices.UseStoredProcedure },
                {"CompetitorPrices.ShouldCompareWithOldCode", CoreSettings.RepositorySettings.CompetitorPrices.ShouldCompareWithOldCode },
                {"CompetitorPrices.CompareOutputFilename", CoreSettings.RepositorySettings.CompetitorPrices.CompareOutputFilename },
            };

            model.Environment = SortDictionaryByKey(environment);
            model.AppSettings = SortDictionaryByKey(appSettings);
            model.CoreSettings = SortDictionaryByKey(coreSettings);

            model.RecentDatabaseObjectsChanges = _db.GetDiagnosticsRecentDatabaseObjectChanges(daysAgo).ToList();
            model.DatabaseObjectSummary = _db.GetDiagnosticsDatabaseObjectSummary();

            return model;
        }

        public bool UpdateDiagnosticsSettings(DiagnosticsSettingsViewModel settings)
        {
            CoreSettings.RepositorySettings.Dapper.LogDapperCalls = settings.Dapper_LogDatabaseCalls;
            CoreSettings.RepositorySettings.SitePrices.UseStoredProcedure = settings.SitePrices_UseStoredProcedure;
            CoreSettings.RepositorySettings.CompetitorPrices.UseStoredProcedure = settings.CompetitorPrices_UseStoredProcedure;
            return true;
        }

        public bool ClearDiagnosticsLog()
        {
            DiagnosticLog.Clear();
            return true;
        }

        #endregion implementation of IDiagnosticsService

        private Dictionary<string, string> SortDictionaryByKey(Dictionary<string, object> dictionary)
        {
            var sorted = new Dictionary<string, string>();
            foreach (var kvp in dictionary)
                sorted.Add(kvp.Key, kvp.Value == null ? "NULL" : kvp.Value.ToString());

            return sorted;
        }
    }
}