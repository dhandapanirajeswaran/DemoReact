using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class DiagnosticsSettingsViewModel
    {
        public bool Dapper_LogDatabaseCalls { get; set; }
        public bool SitePrices_UseStoredProcedure { get; set; }
        public bool CompetitorPrices_UseStoredProcedure { get; set; }
    }
}
