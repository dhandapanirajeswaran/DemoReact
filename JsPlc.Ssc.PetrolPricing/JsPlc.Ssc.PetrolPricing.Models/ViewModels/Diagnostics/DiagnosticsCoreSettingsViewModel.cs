using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsSettingsViewModel
    {
        [Display(Name = "Log Database Calls")]
        public bool Dapper_LogDatabaseCalls { get; set; } = false;
        [Display(Name = "Log Information Messages")]
        public bool Logging_LogInformationMessages { get; set; } = false;
        [Display(Name = "Log Debug Messages")]
        public bool Logging_LogDebugMessages { get; set; } = false;
        [Display(Name = "Log Trace Message")]
        public bool Logging_LogTraceMessages { get; set; } = false;
        [Display(Name = "Data Cleanse Files After Days")]
        public int DataCleanseFilesAfterDays { get; set; } = 60;
    }
}
