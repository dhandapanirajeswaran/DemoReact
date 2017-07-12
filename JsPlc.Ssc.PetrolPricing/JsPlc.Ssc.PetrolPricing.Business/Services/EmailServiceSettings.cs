using JsPlc.Ssc.PetrolPricing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class EmailServiceSettings
    {
        protected readonly IFactory _factory;
        protected readonly IAppSettings _appSettings;

        public EmailServiceSettings(IAppSettings appSettings,
            IFactory factory)
        {
            _factory = factory;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Creates an SMTP Client based on AppSettings Keys
        /// </summary>
        /// <returns></returns>
        public ISmtpClient CreateSmtpClient()
        {
            // Localhost, Gmail, AWS
            var mailHostSelector = _appSettings.MailHostSelector;

            var client = _factory.Create<ISmtpClient>(CreationMethod.ServiceLocator, null);

            switch (mailHostSelector.ToUpper())
            {
                case "LOCALHOST":
                    {
                        // Mail Delivery working on a VM box:
                        // Server localhost:25, (.eml) email appears in MailRoot/Drop
                        // Smtp Server Domains = Alias domain = gmail.com
                        // UseDefaultCredentials = true; EnableSsl = false;
                        client.Host = "localhost";
                        client.Port = 25;
                        client.EnableSsl = false;
                        client.UseDefaultCredentials = true;
                    }
                    break;

                case "GMAIL":
                    {
                        client.Host = "smtp.gmail.com";
                        client.Port = 587; // 25 or 465 (with SSL) and port 587 (with TLS)
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(
                            userName: "akiaip5@gmail.com",
                            password: "AmDoy02X");
                    }
                    break;

                case "AWS":
                    {
                        client.Host = _appSettings.SmtpServer;
                        client.Port = _appSettings.SmtpPort;
                        client.EnableSsl = _appSettings.SmtpEnableSsl;
                        client.Credentials = new NetworkCredential(
                            userName: _appSettings.SmtpUserName,
                            password: _appSettings.SmtpPassword);
                    }
                    break;
            }
            return client;
        }
    }
}