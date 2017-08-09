using JsPlc.Ssc.PetrolPricing.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.Core.Diagnostics
{
    public class DiagnoticsLogEntry
    {
        public DateTime Created { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

    public static class DiagnosticLog
    {
        private const int MaxLogEntries = 100;

        public static List<DiagnoticsLogEntry> Entries = new List<DiagnoticsLogEntry>();

        public static void AddLog(string level, string message, Dictionary<string, string> parameters = null, string exception = "")
        {

            if (String.Compare(level, "Debug", true) == 0 && !CoreSettings.Logging.LogDebugMessages)
                return;

            if (String.Compare(level, "Information", true) == 0 && !CoreSettings.Logging.LogInformationMessages)
                return;

            if (String.Compare(level, "Trace", true) == 0 && !CoreSettings.Logging.LogTraceMessages)
                return;

            var entry = new DiagnoticsLogEntry()
            {
                Created = DateTime.Now,
                Level = String.IsNullOrWhiteSpace(level) ? "N/A" : level,
                Message = String.IsNullOrWhiteSpace(message) ? "N/A" : message,
                Exception = String.IsNullOrWhiteSpace(exception) ? "N/A" : exception,
                Parameters = CloneParameters(parameters)
            };

            if (Entries.Count() > MaxLogEntries) {
                var deleteCount = Entries.Count() - MaxLogEntries;
                Entries = Entries.Skip(deleteCount).Take(MaxLogEntries).ToList();
            }
            Entries.Add(entry);
        }

        public static void Clear()
        {
            Entries.Clear();
        }

        public static void StartDebug(string message, Dictionary<string,string> parameters = null, string exception = "")
        {
            AddLog("Debug", String.Format("Started: {0}", message), parameters, exception);
        }

        public static void EndDebug(string message)
        {
            AddLog("Debug", String.Format("Finished: {0}", message));
        }

        public static void FailedDebug(string message, Dictionary<string, string> parameters = null, string exception = "")
        {
            AddLog("Debug", String.Format("Failed: {0}", message), parameters, exception);
        }

        public static IEnumerable<DiagnoticsLogEntry> CloneLogEntries()
        {
            return Entries.Select(
                x => new DiagnoticsLogEntry()
                {
                    Created = x.Created,
                    Level = x.Level,
                    Message = x.Message,
                    Exception = x.Exception,
                    Parameters = x.Parameters
                }
                ).ToList();
        }

        #region private methods

        private static Dictionary<string, string> CloneParameters(Dictionary<string, string> parameters)
        {
            var clone = new Dictionary<string, string>();
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    clone.Add(kvp.Key, kvp.Value ?? "");
            }
            return clone;
        }

        #endregion private methods
    }
}