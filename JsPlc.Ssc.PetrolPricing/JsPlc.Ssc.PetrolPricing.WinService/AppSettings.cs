using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;

namespace JsPlc.Ssc.PetrolPricing.WinService
{
    public static class AppSettings
    {

        public static string WebApiServiceBaseUrl
        {
            get { return GetAsString("WebApiServiceBaseUrl", "", true); }
        }

        public static int PollSettingsEveryMinutes
        {
            get { return GetAsInteger("PollSettingsEveryMinutes", 5, true); }
        }

        #region private methods

        private static string GetAsString(string key, string defaultValue, bool required)
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[key];
            if (String.IsNullOrWhiteSpace(value))
            {
                if (required)
                    DebugLogger.Error("Missing AppSetting key: " + key);
                return defaultValue;
            }
            return value;
        }

        private static int GetAsInteger(string key, int defaultValue, bool required = false)
        {
            var value = GetAsString(key, "", required);
            if (String.IsNullOrWhiteSpace(value))
                return defaultValue;

            int intValue;
            return int.TryParse(value, out intValue)
                ? intValue
                : defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue, bool required = false)
        {
            var value = GetAsString(key, "", required);
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