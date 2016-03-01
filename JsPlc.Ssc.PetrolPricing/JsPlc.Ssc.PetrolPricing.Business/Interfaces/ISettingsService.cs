using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ISettingsService
    {
        string GetSetting(string settingKey);

        string GetUploadPath();

        string ExcelFileSheetName();

        string EmailSubject();

        string EmailFrom();

        string FixedEmailTo();

        string MailHostSelector();

        string GetSuperUnleadedMarkup();

        string PetrolDbConnectionString();
    }
}
