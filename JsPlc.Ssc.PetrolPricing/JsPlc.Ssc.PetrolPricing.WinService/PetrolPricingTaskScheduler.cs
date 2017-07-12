using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;
using JsPlc.Ssc.PetrolPricing.WinService.Facade;

namespace JsPlc.Ssc.PetrolPricing.WinService
{
    public class PetrolPricingTaskScheduler : IPetrolPricingTaskScheduler
    {
        public void Start()
        {
            DebugLogger.Info("OnStart()");
            DebugLogger.Info(AppSettings.WebApiServiceBaseUrl);

            DebugLogger.Info("Trying...");
            var facade = new WebApiFacade();
            var model = facade.GetWinServiceScheduledItems();
            //DebugLogger.Info(model.First().EmailAddress);
        }

        public void Stop()
        {
            DebugLogger.Info("OnStop()");
        }
    }
}