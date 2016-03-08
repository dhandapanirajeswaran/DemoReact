using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public interface ISmtpClient : IDisposable
	{
		string Host { get; set; }
		int Port { get; set; }
		bool EnableSsl { get; set; }
		bool UseDefaultCredentials { get; set; }
		NetworkCredential Credentials { get; set; }

		void Send(MailMessage message);
	}
}
