using JsPlc.Ssc.PetrolPricing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public static class SimpleScheduler
    {
        private delegate void Worker();
        private static Thread _worker;

        private static TimeSpan _interval = TimeSpan.FromMinutes(1);
        private static Action _action;

        private static object _locker = new object();
        private static bool _isRunning = false;
        private static DateTime? _lastStarted;
        private static DateTime? _lastStopped;
        private static DateTime? _lastPolled;
        private static DateTime? _lastErrored;
        private static string _lastErrorMessage = "";

        public static void Start(TimeSpan interval, Action action)
        {
            _interval = interval;
            _action = action;

            if (_isRunning)
                return;

            if (_worker == null)
                _worker = new Thread(new ThreadStart(DoWork));

            _isRunning = true;
            _lastStarted = DateTime.Now;
            _worker.Start();
        }

        public static void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _worker.Abort();
            _lastStopped = DateTime.Now;
        }

        public static SchedulerStatus GetStatus()
        {
            return new SchedulerStatus()
            {
                IsRunning = _isRunning,
                LastStarted = _lastStarted,
                LastStopped = _lastStopped,
                LastPolled = _lastPolled
            };
        }

        private static void DoWork()
        {
            var running = true;
            TimeSpan interval;

            while (running)
            {
                lock (_locker)
                {
                    running = _isRunning;
                    _lastPolled = DateTime.Now;
                    interval = _interval;
                }

                try
                {
                    _action();
                }
                catch (Exception ex)
                {
                    lock(_locker)
                    {
                        _lastErrored = DateTime.Now;
                        _lastErrorMessage = ex.ToString();
                    }
                }

                Thread.Sleep(interval);
            }
        }
    }
}