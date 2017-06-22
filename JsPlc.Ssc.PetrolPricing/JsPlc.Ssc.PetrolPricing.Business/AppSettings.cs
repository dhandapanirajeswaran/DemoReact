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
	public class AppSettings : IAppSettings
	{
		public string UploadPath 
		{
			get
			{
				return ConfigurationManager.AppSettings["UploadPath"];
			}
		}

		public string ExcelFileSheetName
		{
			get
			{
				return ConfigurationManager.AppSettings["ExcelQuarterlyFileSheetName"];
			}
		}

		public string EmailSubject
		{
			get
			{
				return ConfigurationManager.AppSettings["EmailSubject"];
			}
		}

		public string EmailFrom
		{
			get
			{
				return ConfigurationManager.AppSettings["EmailFrom"];
			}
		}

		public string FixedEmailTo
		{
			get
			{
				return ConfigurationManager.AppSettings["FixedEmailTo"];
			}
		}

		public string MailHostSelector
		{
			get
			{
				return ConfigurationManager.AppSettings["MailHostSelector"];
			}
		}

		public int SuperUnleadedMarkup
		{
			get
			{
				return int.Parse(ConfigurationManager.AppSettings["SuperUnleadedMarkup"]);
			}
		}

		public string PetrolDbConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["PetrolPricingRepository"].ToString();
			}
		}

		public string SmtpServer
		{
			get
			{
				return ConfigurationManager.AppSettings["SmtpServer"];
			}
		}

		public string SmtpUserName
		{
			get
			{
				return ConfigurationManager.AppSettings["SmtpUserName"];
			}
		}

		public string SmtpPassword
		{
			get
			{
				return ConfigurationManager.AppSettings["SmtpPassword"];
			}
		}

		public int SmtpPort
		{
			get
			{
				return int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
			}
		}

		public bool SmtpEnableSsl
		{
			get
			{
				return bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);
			}
		}


		public string GetSetting(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

        public string LogFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["LogFilePath"];
            }
        }
    }
}