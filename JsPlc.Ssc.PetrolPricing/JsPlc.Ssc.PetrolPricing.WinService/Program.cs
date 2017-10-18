using JsPlc.Ssc.PetrolPricing.WinService.Scheduler;
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
        // Installation
        // ============
        // (1) Build the project
        // (2) run the "install.bat" file in the /bin/ folder
        // (3) start the "Petrol Pricing Win Service" in Control Panel -> Services
        //
        // Removal
        // =======
        // (1) stop the "Petrol Pricing Win Service" in Control Panel -> Services
        // (2) run the "uninstall.bat" file in the /bin/ folder
        //
        // Events
        // ======
        // (1) Open the Control Panel -> Events
        // (2) Expand the Windows Logs > Application node in Event Viewer (Local) pane

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;

            if (Environment.UserInteractive)
            {
                // not sure if this is required
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