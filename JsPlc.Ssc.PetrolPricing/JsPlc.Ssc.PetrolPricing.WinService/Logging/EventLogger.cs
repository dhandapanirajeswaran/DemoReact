using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;

namespace JsPlc.Ssc.PetrolPricing.WinService.Logging
{
    public class EventLogger : IEventLog
    {
        private EventLog _eventLog;
        private string _name;

        public bool EnableTrace { get; set; } = false;
        public bool EnableDebug { get; set; } = false;
        public bool EnableInfo { get; set; } = true;

        public EventLogger(EventLog eventLog, string name)
        {
            _eventLog = eventLog;
            _name = name;
        }

        public IEventLog Context(string name)
        {
            var logger = new EventLogger(_eventLog, _name + " > " + name);
            logger.EnableDebug = this.EnableDebug;
            logger.EnableInfo = this.EnableInfo;
            logger.EnableTrace = this.EnableTrace;
            return logger;
        }

        public void Info(string message)
        {
            if (this.EnableInfo)
                _eventLog.WriteEntry(BuildMessage("[INFO]", message), EventLogEntryType.Information);
        }

        public void Error(string message)
        {
            _eventLog.WriteEntry(BuildMessage("[ERROR]",message), EventLogEntryType.Error);
        }

        public void Exception(string message, Exception ex)
        {
            _eventLog.WriteEntry(BuildMessage("[EXCEPTION]", message), EventLogEntryType.Error);
        }

        public void Trace(string message)
        {
            if (this.EnableTrace)
                _eventLog.WriteEntry(BuildMessage("[TRACE]", message), EventLogEntryType.Information);
        }

        public void Debug(string message)
        {
            if (this.EnableDebug)
                _eventLog.WriteEntry(BuildMessage("[DEBUG]", message), EventLogEntryType.Information);
        }

        public void Dispose()
        {
            // nothing to see - allow 'using' statement
        }

        private string BuildMessage(string prefix, string message)
        {
            return string.Format("{0} {1} :: {2}", prefix, _name, message);
        }
    }
}