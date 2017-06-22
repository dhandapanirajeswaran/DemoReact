using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business.Interfaces
{
    public interface IDiagnosticsService
    {
        DiagnosticsViewModel GetDiagnostics(int daysAgo, string logFilePath);
        bool UpdateDiagnosticsSettings(DiagnosticsSettingsViewModel settings);
        bool ClearDiagnosticsLog();

        bool DeleteAllData();

        IEnumerable<DiagnosticsRecordCountViewModel> GetDatabaseRecordCounts();

        DataSanityCheckSummaryViewModel GetDataSanityCheckSummary();

        DiagnosticsErrorLogFileViewModel GetErrorLogFile(string logFilePath, string filename);
    }
}
