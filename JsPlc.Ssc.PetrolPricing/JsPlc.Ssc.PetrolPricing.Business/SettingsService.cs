using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}