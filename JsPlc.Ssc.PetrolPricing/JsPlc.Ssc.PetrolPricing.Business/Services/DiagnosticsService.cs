using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.IO;
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

            var systemSettings = _db.GetSystemSettings();

            var model = new DiagnosticsViewModel()
            {
                DiagnosticsSettings = new DiagnosticsSettingsViewModel()
                {
                    Logging_LogDebugMessages = CoreSettings.Logging.LogDebugMessages,
                    Logging_LogInformationMessages = CoreSettings.Logging.LogInformationMessages,
                    Dapper_LogDatabaseCalls = CoreSettings.RepositorySettings.Dapper.LogDapperCalls,
                    SitePrices_UseStoredProcedure = CoreSettings.RepositorySettings.SitePrices.UseStoredProcedure,
                    CompetitorPrices_UseStoredProcedure = CoreSettings.RepositorySettings.CompetitorPrices.UseStoredProcedure,
                    DataCleanseFilesAfterDays = systemSettings.DataCleanseFilesAfterDays
                }
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
                {"appSetting.UploadPath", _appSettings.UploadPath }
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

            var sysSettings = new Dictionary<string, object>()
            {
                {"DataCleanseFilesAfterDays", systemSettings.DataCleanseFilesAfterDays },
                {"LastDataCleanseFilesOn", systemSettings.LastDataCleanseFilesOn }
            };

            model.Environment = SortDictionaryByKey(environment);
            model.AppSettings = SortDictionaryByKey(appSettings);
            model.CoreSettings = SortDictionaryByKey(coreSettings);
            model.SystemSettings = SortDictionaryByKey(sysSettings);

            model.RecentDatabaseObjectsChanges = _db.GetDiagnosticsRecentDatabaseObjectChanges(daysAgo).ToList();
            model.DatabaseObjectSummary = _db.GetDiagnosticsDatabaseObjectSummary();

            model.FileUploadSummary = GetFileUploadsSummary(_appSettings.UploadPath);

            return model;
        }

        public bool UpdateDiagnosticsSettings(DiagnosticsSettingsViewModel settings)
        {
            CoreSettings.Logging.LogDebugMessages = settings.Logging_LogDebugMessages;
            CoreSettings.Logging.LogInformationMessages = settings.Logging_LogInformationMessages;

            CoreSettings.RepositorySettings.Dapper.LogDapperCalls = settings.Dapper_LogDatabaseCalls;
            CoreSettings.RepositorySettings.SitePrices.UseStoredProcedure = settings.SitePrices_UseStoredProcedure;
            CoreSettings.RepositorySettings.CompetitorPrices.UseStoredProcedure = settings.CompetitorPrices_UseStoredProcedure;

            var systemSettings = _db.GetSystemSettings();

            if (settings.DataCleanseFilesAfterDays >= Const.MinDataCleanseFilesAfterDays)
            {
                systemSettings.DataCleanseFilesAfterDays = settings.DataCleanseFilesAfterDays;
                _db.UpdateSystemSettings(systemSettings);
            }

            return true;
        }

        public bool ClearDiagnosticsLog()
        {
            DiagnosticLog.Clear();
            return true;
        }

        public bool DeleteAllData()
        {
            try
            {
                var result = _db.DeleteAllData(_appSettings.UploadPath);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in DeleteAllData" + Environment.NewLine + ex.Message, ex);
            }
        }

        #endregion implementation of IDiagnosticsService

        private Dictionary<string, string> SortDictionaryByKey(Dictionary<string, object> dictionary)
        {
            var sorted = new Dictionary<string, string>();
            foreach (var kvp in dictionary)
                sorted.Add(kvp.Key, kvp.Value == null ? "NULL" : kvp.Value.ToString());

            return sorted;
        }

        private DiagnosticsFileUploadSummaryViewModel GetFileUploadsSummary(string uploadPath)
        {
            var model = new DiagnosticsFileUploadSummaryViewModel();

            try
            {
                var now = DateTime.Now.Date;
                var directory = new DirectoryInfo(uploadPath);
                var files = directory.GetFiles().ToList();
                foreach(var file in files)
                {
                    var lastWriteTime = file.LastWriteTime;

                    model.TotalFileCount++;
                    model.TotalFileSize += file.Length;

                    if (!model.OldestDateTime.HasValue || lastWriteTime < model.OldestDateTime)
                        model.OldestDateTime = lastWriteTime;
                    if (!model.NewestDateTime.HasValue || lastWriteTime > model.NewestDateTime)
                        model.NewestDateTime = lastWriteTime;

                    var daysAgo = now.Subtract(lastWriteTime.Date).TotalDays;

                    if (daysAgo < 7)
                        model.FilesInLast7Days++;
                    else if (daysAgo < 30)
                        model.FilesOlderThan7Days++;
                    else if (daysAgo < 60)
                        model.FilesOlderThan30Days++;
                    else if (daysAgo < 90)
                        model.FilesOlderThan60Days++;
                    else if (daysAgo < 365)
                        model.FilesOlderThan90Days++;
                    else
                        model.FilesOlderThan1Year++;
                }
            }
            catch (Exception ex)
            {
            }

            return model;
        }
    }
}