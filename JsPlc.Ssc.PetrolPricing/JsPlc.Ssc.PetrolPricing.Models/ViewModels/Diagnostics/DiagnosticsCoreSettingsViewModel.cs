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
        [Display(Name="Log Database Calls")]
        public bool Dapper_LogDatabaseCalls { get; set; }
        [Display(Name="Use Stored Procedure")]
        public bool SitePrices_UseStoredProcedure { get; set; }
        [Display(Name = "Use Stored Procedure")]
        public bool CompetitorPrices_UseStoredProcedure { get; set; }
        [Display(Name = "Log Information Messages")]
        public bool Logging_LogInformationMessages { get; set; }
        [Display(Name = "Log Debug Messages")]
        public bool Logging_LogDebugMessages { get; set; }

        public DiagnosticsSettingsViewModel()
        {
            this.Dapper_LogDatabaseCalls = false;
            this.SitePrices_UseStoredProcedure = false;
            this.CompetitorPrices_UseStoredProcedure = false;
            this.Logging_LogDebugMessages = false;
            this.Logging_LogInformationMessages = false;
        }
    }
}
