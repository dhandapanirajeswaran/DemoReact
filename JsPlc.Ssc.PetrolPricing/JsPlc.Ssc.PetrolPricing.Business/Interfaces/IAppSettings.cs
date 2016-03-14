using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public interface IAppSettings
	{
		string UploadPath
		{
			get;
		}

		string ExcelFileSheetName
		{
			get;
		}

		string EmailSubject
		{
			get;
		}

		string EmailFrom
		{
			get;
		}

		string FixedEmailTo
		{
			get;
		}

		string MailHostSelector
		{
			get;
		}

		int SuperUnleadedMarkup
		{
			get;
		}

		string PetrolDbConnectionString
		{
			get;
		}

		string SmtpServer
		{
			get;
		}

		string SmtpUserName
		{
			get;
		}

		string SmtpPassword
		{
			get;
		}

		int SmtpPort
		{
			get;
		}

		bool SmtpEnableSsl
		{
			get;
		}

		string GetSetting(string key);
	}
}
