using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.Diagnostics
{
    public class DiagnoticsLogEntry
    {
        public DateTime Created { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }

    public static class DiagnosticLog
    {
        const int MaxLogEntries = 30;

        public static List<DiagnoticsLogEntry> Entries = new List<DiagnoticsLogEntry>();

        public static void AddLog(string level, string message, string exception = "")
        {
            var entry = new DiagnoticsLogEntry()
            {
                Created = DateTime.Now,
                Level = String.IsNullOrWhiteSpace(level) ? "N/A" : level,
                Message = String.IsNullOrWhiteSpace(message) ? "N/A" : message,
                Exception = String.IsNullOrWhiteSpace(exception) ? "N/A" : exception
            };

            Entries = Entries.Take(MaxLogEntries).ToList();
            Entries.Add(entry);
        }

        public static void StartDebug(string message)
        {
            AddLog("Debug", String.Format("Started: {0}", message));
        }

        public static void EndDebug(string message)
        {
            AddLog("Debug", String.Format("Finished: {0}", message));
        }

        public static void FailedDebug(string message, string exception = "")
        {
            AddLog("Debug", String.Format("Failed: {0}", message), exception);
        }

        public static IEnumerable<DiagnoticsLogEntry> CloneLogEntries() 
        {
            return Entries.Select(
                x => new DiagnoticsLogEntry()
                {
                    Created = x.Created,
                    Level = x.Level,
                    Message = x.Message,
                    Exception = x.Exception
                }
                ).ToList();
        }
    }
}
