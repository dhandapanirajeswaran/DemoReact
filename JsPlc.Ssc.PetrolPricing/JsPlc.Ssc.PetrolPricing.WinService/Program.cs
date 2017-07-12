using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.WinService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;

            if (Environment.UserInteractive)
            {
                // todo
            }
            else
            {
                ServicesToRun = new ServiceBase[]
                {
                    new PetrolPricingWinService(new PetrolPricingTaskScheduler())
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}