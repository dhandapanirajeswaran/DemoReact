using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsViewModel
    {
        public string ActionMessage { get; set; }

        public string ApiExceptionMessage { get; set; }

        public DiagnosticsSettingsViewModel DiagnosticsSettings;

        public DataSanityCheckSummaryViewModel DataSanityCheck;

        public Dictionary<string, string> AppSettings;
        public Dictionary<string, string> CoreSettings;
        public Dictionary<string, string> Environment;
        public Dictionary<string, string> SystemSettings;

        public List<DiagnosticsLogEntryViewModel> LogEntries;

        public List<DiagnosticsDatabaseObject> RecentDatabaseObjectsChanges;
        public DiagnosticsDatabaseObjectSummary DatabaseObjectSummary;
        public DiagnosticsFileUploadSummaryViewModel FileUploadSummary;

        public List<DiagnosticsRecordCountViewModel> DatabaseRecordCounts;

        public DiagnosticsViewModel()
        {
            this.ActionMessage = "";
            this.ApiExceptionMessage = "";
            this.DataSanityCheck = new DataSanityCheckSummaryViewModel();
            this.DiagnosticsSettings = new DiagnosticsSettingsViewModel();
            this.AppSettings = new Dictionary<string, string>();
            this.CoreSettings = new Dictionary<string, string>();
            this.Environment = new Dictionary<string, string>();
            this.SystemSettings = new Dictionary<string, string>();
            this.LogEntries = new List<DiagnosticsLogEntryViewModel>();
            this.RecentDatabaseObjectsChanges = new List<DiagnosticsDatabaseObject>();
            this.DatabaseObjectSummary = new DiagnosticsDatabaseObjectSummary();
            this.FileUploadSummary = new DiagnosticsFileUploadSummaryViewModel();
            this.DatabaseRecordCounts = new List<DiagnosticsRecordCountViewModel>();
        }
    }
}
