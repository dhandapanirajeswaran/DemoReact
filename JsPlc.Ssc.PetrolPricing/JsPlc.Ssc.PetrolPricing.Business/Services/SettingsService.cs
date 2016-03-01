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
    public class SettingsService : ISettingsService
    {
        protected readonly IPetrolPricingRepository _db;

        public SettingsService(IPetrolPricingRepository db)
        {
            _db = db;
        }

        public string GetSetting(string settingKey)
        {
            var appConfigSettings = _db.GetAppConfigSettings().FirstOrDefault(x => x.SettingKey.Equals(settingKey, StringComparison.CurrentCultureIgnoreCase));
            if (appConfigSettings == null)
            {
                throw new ApplicationException("ServiceException: Unable to retrieve relevant setting value:" + settingKey);
            }
            return appConfigSettings.SettingValue;
        }

        public string GetUploadPath()
        {
            return GetSetting("UploadPath");
        }

        public string ExcelFileSheetName()
        {
            return GetSetting("ExcelQuarterlyFileSheetName");
        }

        public string EmailSubject()
        {
            return GetSetting("emailSubject");
        }

        public string EmailFrom()
        {
            return GetSetting("emailFrom");
        }
        
        public string FixedEmailTo() // For internal testing, set this value in config and run Admin script to set configkeys to DB
        {
            return GetSetting("emailTo");
        }
        
        public string MailHostSelector()
        {
            return GetSetting("mailHostSelector");
        }

        public string GetSuperUnleadedMarkup()
        {
            return GetSetting("SuperUnleadedMarkup");
        }

        public string PetrolDbConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PetrolPricingRepository"].ToString();
        }

    }
}