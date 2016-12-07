using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using ILogger = JsPlc.Ssc.PetrolPricing.Core.Interfaces.ILogger;


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
        }
		
    }
}
