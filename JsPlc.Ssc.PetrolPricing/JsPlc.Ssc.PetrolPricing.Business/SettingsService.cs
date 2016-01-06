using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public static class SettingsService
    {
        public static string GetSetting(string settingKey)
        {
            using (var db = new PetrolPricingRepository(new RepositoryContext()))
            {
                var appConfigSettings = db.GetAppConfigSettings().FirstOrDefault(x =>x.SettingKey.Equals(settingKey, StringComparison.CurrentCultureIgnoreCase));

                if (appConfigSettings == null)
                {
                    throw new ApplicationException("ServiceException: Unable to retrieve relevant setting value:" + settingKey);
                }

                return appConfigSettings.SettingValue;
            }
        }

        public static string GetUploadPath()
        {
            return GetSetting("UploadPath");
        }

        public static int GetImportTimeoutMilliSecs()
        {
            int timeoutMin = 1;
            Int32.TryParse(GetSetting("ImportTimeoutMin"), out timeoutMin);
            return timeoutMin*60*1000; // min to milliseconds
        }

        public static int GetCalcTimeoutMilliSecs()
        {
            int timeoutMin = 1;
            Int32.TryParse(GetSetting("DailyCalcTimeoutMin"), out timeoutMin);
            return timeoutMin*60*1000; // min to milliseconds
        }

        public static string ExcelFileSheetName()
        {
            return GetSetting("ExcelQuarterlyFileSheetName");
        }

        public static string EmailSubject()
        {
            return GetSetting("emailSubject");
        }
        public static string EmailFrom()
        {
            return GetSetting("emailFrom");
        }
        public static string FixedEmailTo() // For internal testing, set this value in config and run Admin script to set configkeys to DB
        {
            return GetSetting("emailTo");
        }
        public static string MailHostSelector()
        {
            return GetSetting("mailHostSelector");
        }

        public static string GetSuperUnleadedMarkup()
        {
            return GetSetting("SuperUnleadedMarkup");
        }

        public static string PetrolDbConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PetrolPricingRepository"].ToString();
        }

    }
}