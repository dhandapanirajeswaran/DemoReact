using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using ILogger = JsPlc.Ssc.PetrolPricing.Core.Interfaces.ILogger;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    public class PetrolPricingLogger : ILogger
    {
        private readonly Logger _internalLogger;

         public PetrolPricingLogger()
        {
            _internalLogger = LogManager.GetCurrentClassLogger();
             
        }

        public void Error(Exception ex)
        {
             _internalLogger.Log
                 (
                     new LogEventInfo
                     {
                         Level = LogLevel.Error,
                         Exception = ex,
                         Message = ex.Message
                     }
                 );

            DiagnosticLog.AddLog(LogLevel.Error.ToString(), ex.Message, null, ex.ToString());
        }

        public void Information(string message)
        {
            _internalLogger.Log
                (
                    new LogEventInfo
                    {
                        Level = LogLevel.Info,
                        Exception = new Exception(message),
                        Message = message
                    }
                );

            DiagnosticLog.AddLog(LogLevel.Info.ToString(), message, null, "");
        }

        public void Debug(string message)
        {
            _internalLogger.Log
                (
                new LogEventInfo
                {
                    Level = LogLevel.Debug,
                    Exception = new Exception(message),
                    Message = message
                }
            );

            DiagnosticLog.AddLog(LogLevel.Debug.ToString(), message, null, "");
        }
    }
}
