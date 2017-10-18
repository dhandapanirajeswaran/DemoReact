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
using JsPlc.Ssc.PetrolPricing.WinService.Settings;

namespace JsPlc.Ssc.PetrolPricing.WinService
{
    public partial class PetrolPricingWinService : ServiceBase
    {
        IPetrolPricingTaskScheduler _scheduler;
        IAppSettings _settings;

        private IEventLog _logger;

        public PetrolPricingWinService(IPetrolPricingTaskScheduler schedule)
        {
            InitializeComponent();
            this.ServiceName = "PetrolPricingWinService";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            _logger = InitEventLog();

            _settings = new AppSettings(_logger);
            _logger.EnableDebug = _settings.EnableDebugLog;
            _logger.EnableInfo = _settings.EnableInfoLog;
            _logger.EnableTrace = _settings.EnableTraceLog;

            _scheduler = schedule;
        }

        private EventLogger InitEventLog()
        {
            ((ISupportInitialize)this.EventLog).BeginInit();
            if (!EventLog.SourceExists(this.ServiceName))
                EventLog.CreateEventSource(this.ServiceName, "Application");
            ((ISupportInitialize)this.EventLog).EndInit();

            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";

            return new EventLogger(this.EventLog, "PetrolPricingWinService");
        }

        protected override void OnStart(string[] args)
        {
            using (var log = _logger.Context("OnStart()"))
            {
                _scheduler.Start(log, _settings);
            }
        }

        protected override void OnStop()
        {
            using (var log = _logger.Context("OnStop()"))
            {
                _scheduler.Stop(log, _settings);
            }
        }
    }
}
