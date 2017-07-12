using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JsPlc.Ssc.PetrolPricing.WinService.Logging
{
    internal static class DebugLogger
    {
        private static EventLog _eventLog;

        internal static void Init(EventLog eventLog)
        {
            _eventLog = eventLog;
        }

        public static void Info(string message)
        {
            _eventLog.WriteEntry(message, EventLogEntryType.Information);
        }
        public static void Error(string message)
        {
            _eventLog.WriteEntry(message, EventLogEntryType.Error);
        }
        public static void Exception(string message, Exception ex)
        {
            _eventLog.WriteEntry(message, EventLogEntryType.Error);
        }
    }
}
