using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public class SmtpClientWrapper : ISmtpClient
	{
		SmtpClient _client = new SmtpClient();

		public string Host
		{
			get
			{
				return _client.Host;
			}
			set
			{
				_client.Host = value;
			}
		}

		public int Port
		{
			get
			{
				return _client.Port;
			}
			set
			{
				_client.Port = value;
			}
		}

		public bool EnableSsl
		{
			get
			{
				return _client.EnableSsl;
			}
			set
			{
				_client.EnableSsl = value;
			}
		}

		public bool UseDefaultCredentials
		{
			get
			{
				return _client.UseDefaultCredentials;
			}
			set
			{
				_client.UseDefaultCredentials = value;
			}
		}

		public System.Net.NetworkCredential Credentials
		{
			get
			{
				return _client.Credentials as System.Net.NetworkCredential;
			}
			set
			{
				_client.Credentials = value;
			}
		}

		public void Send(System.Net.Mail.MailMessage message)
		{
			_client.Send(message);
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
