using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;
using JsPlc.Ssc.PetrolPricing.WinService.Facade;
using System.Timers;

namespace JsPlc.Ssc.PetrolPricing.WinService.Scheduler
{
    public class PetrolPricingTaskScheduler : IPetrolPricingTaskScheduler
    {
        private Timer _timer;

        private IEventLog _timerLog;
        private IAppSettings _settings;

        public void Start(IEventLog eventLog, IAppSettings settings)
        {
            using (var log = eventLog.Context("PetrolPricingTaskScheduler.Start()"))
            {
                log.Debug("Setup timer - started");

                SetupTimer(log, settings);

                log.Debug("Setup timer - complete");
            }
        }

        public void Stop(IEventLog eventLog, IAppSettings settings)
        {
            using (var log = eventLog.Context("PetrolPricingTaskScheduler.Stop()"))
            {
                log.Info("Testing - Stop()");

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
            }
        }

        private void SetupTimer(IEventLog log, IAppSettings settings)
        {
            _settings = settings;

            // create Log context for the timer event
            _timerLog = log.Context("Timer");

            // calculate the timer interval (in milliseconds)
            var interval = TimeSpan.FromMinutes(settings.RunEmailSecheduleEveryXMinutes);

            _timer = new Timer();
            _timer.Interval = interval.TotalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        /// <summary>
        /// Event trigger on every timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerLog.Debug("Timer tick at " + DateTime.Now);

            using (var apiLog = _timerLog.Context("WebApiFacade"))
            {
                apiLog.Trace("ExecuteWinServiceSchedule - starting");

                var facade = new WebApiFacade(apiLog, _settings);
                facade.ExecuteWinServiceSchedule();

                apiLog.Trace("ExecuteWinServiceSchedule - ended");
            }
        }
    }
}