using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;
using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;

namespace JsPlc.Ssc.PetrolPricing.WinService.Settings
{
    public class AppSettings : IAppSettings
    {
        private IEventLog _logger;

        public AppSettings(IEventLog logger)
        {
            _logger = logger.Context("AppSettings");
        }

        public string ServicesBaseUrl
        {
            get { return GetString(key: "ServicesBaseUrl", defaultValue: "", required: true); }
        }

        public int RunEmailScheduleEveryXMinutes
        {
            get { return GetInteger(key: "RunEmailScheduleEveryXMinutes", defaultValue: 5, required: true); }
        }

        public bool EnableDebugLog
        {
            get { return GetBool(key: "EnableDebugLog", defaultValue: false, required: false); }
        }

        public bool EnableTraceLog
        {
            get { return GetBool(key: "EnableTraceLog", defaultValue: false, required: false); }
        }

        public bool EnableInfoLog
        {
            get { return GetBool(key: "EnableInfoLog", defaultValue: false, required: false); }
        }

        #region private methods

        private string GetString(string key, string defaultValue, bool required)
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[key];
            if (String.IsNullOrWhiteSpace(value))
            {
                if (required)
                    _logger.Error("Missing (App.Config) AppSetting key: " + key);
                return defaultValue;
            }
            return value;
        }

        private int GetInteger(string key, int defaultValue, bool required = false)
        {
            var value = GetString(key, "", required);
            if (String.IsNullOrWhiteSpace(value))
                return defaultValue;

            int intValue;
            return int.TryParse(value, out intValue)
                ? intValue
                : defaultValue;
        }

        private bool GetBool(string key, bool defaultValue, bool required = false)
        {
            var value = GetString(key, "", required);
            if (String.IsNullOrWhiteSpace(value))
                return defaultValue;
            switch (value.ToUpperInvariant())
            {
                case "TRUE":
                case "1":
                case "YES":
                case "ENABLE":
                    return true;

                case "FALSE":
                case "0":
                case "NO":
                case "DISABLE":
                    return false;
            }
            return defaultValue;
        }

        #endregion private methods
    }
}