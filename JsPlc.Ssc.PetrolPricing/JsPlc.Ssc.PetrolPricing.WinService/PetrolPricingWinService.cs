using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;

namespace JsPlc.Ssc.PetrolPricing.WinService
{
    public partial class PetrolPricingWinService : ServiceBase
    {
        IPetrolPricingTaskScheduler _scheduler;

        public PetrolPricingWinService(IPetrolPricingTaskScheduler scheduler)
        {
            InitializeComponent();
            _scheduler = scheduler;
            this.CanStop = true;
            this.CanPauseAndContinue = true;

            InitEventLog();
        }

        protected override void OnStart(string[] args)
        {
            _scheduler.Start();
        }

        protected override void OnStop()
        {
            _scheduler.Stop();
        }

        private void InitEventLog()
        {
            this.AutoLog = false;
            ((ISupportInitialize)this.EventLog).BeginInit();
            if (!EventLog.SourceExists(this.ServiceName))
            {
                EventLog.CreateEventSource(this.ServiceName, "Application");
            }
            ((ISupportInitialize)this.EventLog).EndInit();
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";

            DebugLogger.Init(this.EventLog);
        }
    }
}
