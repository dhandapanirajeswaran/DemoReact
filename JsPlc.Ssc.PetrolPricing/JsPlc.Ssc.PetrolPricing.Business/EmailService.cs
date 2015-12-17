using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System.Net;

using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{

    public class EmailService : BaseService, IDisposable
    {
        public void Dispose()
        {
            // do nothing for now
        }

        //send email to users provided in EmailService list
        public bool SendEmail(IEnumerable<Site> listSites, DateTime endTradeDate)
        {
            try
            {
                foreach (Site site in listSites)
                {
                    //one email built per sites for multiple user in site email list
                    var emailBody = BuildEmailBody(site, endTradeDate);
                    if (String.IsNullOrEmpty(emailBody)) continue;
                    
                    var emailSubject = ConfigurationManager.AppSettings["emailSubject"];
                    var emailFrom = ConfigurationManager.AppSettings["emailFrom"];

                    foreach (var email in site.Emails.Where(x => !string.IsNullOrEmpty(x.EmailAddress)))
                    {
                        // using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["smtpServer"], int.Parse(ConfigurationManager.AppSettings["smtpPort"])))
                        using (var smtpClient = CreateSmtpClient())
                        {
                            try
                            {
                                var emailTo = !String.IsNullOrEmpty(SettingsService.GetSetting("emailTo"))
                                    ? SettingsService.GetSetting("emailTo")
                                    : email.EmailAddress;

                                var message = new MailMessage();
                                message.From = new MailAddress(email.EmailAddress, emailFrom);
                                message.Subject = site.SiteName + " - " + emailSubject;
                                message.To.Add(emailTo);
                                message.Body = emailBody;
                                message.BodyEncoding = Encoding.ASCII;
                                message.IsBodyHtml = true;

                                smtpClient.Send(message);
                            }
                            catch 
                            {
                                RecordEmailsSentToSites("Fail", email.EmailAddress); //FAIL - Not Send
                            }
                        }

                        RecordEmailsSentToSites("Success", email.EmailAddress);//Success - Sent 
                    }
                }
            }
            catch
            {
                RecordEmailsSentToSites("Fail", ""); //FAIL - Could Not Send
                return false;
            }

            return true;
        }

        public static string BuildEmailBody(Site site, DateTime endTradeDate)
        {
            var emailForSite = new EmailSiteData();

            emailForSite = EmailSetValues(site, emailForSite, endTradeDate);

            if (emailForSite == null || !emailForSite.atLeastOnePriceAvailable) return "";

            emailForSite.emailBody = EmailGetLayout();
            emailForSite.emailBody = EmailReplaceTemplateKeys(emailForSite);

            return emailForSite.emailBody;
        }

        //Below helper methods used by build email. 
        private static string EmailGetLayout()
        {
            StringBuilder sb = new StringBuilder();

            //Header
            sb.Append("<h1>FAO Store/duty manager - URGENT FUEL PRICE CHANGE</h1>");
            sb.Append("<p>Queries to the Trading Hotline using Option 1 Trading, Option 2 Grocery and Option 8 Petrol and Kiosk </p>");

            //Body
            sb.Append("<h2>kSiteName</h2>");
            sb.Append("<p><strong>Petrol price changes, effective end of trade kStartDateMonthYear</strong></p>");
            
            //Price Table !important
            sb.Append("<table>");

            sb.Append("<tr><td><strong>Product</strong></td><td><strong>New Price</strong></td></tr>");
            sb.Append("<tr><td>Unleaded</td><td>kUnleadedPrice</td></tr>");
            sb.Append("<tr><td>LPG (if applicable)</td><td>kLpgPrice</td></tr>");
            sb.Append("<tr><td>Diesel</td><td>kDieselPrice</td></tr>");

            sb.Append("</table>");

            //Static
            sb.Append("<ul>");
            sb.Append("<li>All fuel price changes must be actioned at the end of trade only</li>");
            sb.Append("<li>Colleague actioning the price change must ensure the changes have applied on the pumps, enter the time of the change and then sign as confirmation before filing this message at the PFS</li>");
            sb.Append("<li>Ensure you enter the new prices correctly into REPOS e.g. If the price is 129.90ppl, then enter 129.90 in the new price field (make sure there are no fuel sales outstanding on the REPOS system or pumps before making the price change, <strong>ALL</strong> working pumps <strong>MUST</strong> be left <strong>ON</strong> and nozzle placed in the pump holders)</li>");
            sb.Append("<li>You are reminded that only fuel price changes sent to you by email should be actioned. Please ignore REPOS price change reports.</li>");
            sb.Append("</ul>");

            sb.Append("<h3>24 hour sites</h3>");
            sb.Append("<ul>");
            sb.Append("<li>Action the price change in conjunction with the REPOS EOD routine between midnight and 2am. Colleague actioning the price change must ensure that the changes have applied on the pumps, enter the time of the change and then sign as confirmation before filing this message at the PFS.</li>");
            sb.Append("</ul>");

            return sb.ToString();
        }

        /// <summary>
        /// Get SitePrice data for email, returns null if not found
        /// </summary>
        /// <param name="site"></param>
        /// <param name="emailForSite"></param>
        /// <param name="endTradeDate"></param>
        /// <returns></returns>
        private static EmailSiteData EmailSetValues(Site site, EmailSiteData emailForSite, DateTime endTradeDate)
        {
            emailForSite.siteName = site.SiteName;
            emailForSite.changeDate = endTradeDate;
            var pricesWithDateCalcAsEndTradeDate = new List<SitePrice>();
            if (site.Prices!= null && site.Prices.Any())
            {
                pricesWithDateCalcAsEndTradeDate = site.Prices.Where(x => x.DateOfCalc.Date.Equals(endTradeDate.Date)).ToList();
            }

            if (!pricesWithDateCalcAsEndTradeDate.Any())
            {
                emailForSite.atLeastOnePriceAvailable = false;
                return emailForSite;
            }

            var atLeastOne = false;
            foreach (SitePrice sp in pricesWithDateCalcAsEndTradeDate)
            {
                if (sp.FuelTypeId==2) // unleaded
                {
                    emailForSite.priceUnleaded = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                    atLeastOne = true;
                }
                if (sp.FuelTypeId == 7) // lpg
                {
                    emailForSite.priceLpg = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                    atLeastOne = true;
                }
                if (sp.FuelTypeId == 6) // diesel
                {
                    emailForSite.priceDiesel = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                    atLeastOne = true;
                }
                
            }
            emailForSite.atLeastOnePriceAvailable = atLeastOne;
            return emailForSite;
        }

        private static string EmailReplaceTemplateKeys(EmailSiteData emailForSite)
        {
            string xxx = "";//changeDate.DayOfWeek.ToString() + " " + changeDate.Month.ToString(), + " " + changeDate.Year.ToString();

            string emailBody = emailForSite.emailBody;

            emailBody = emailBody.Replace("kSiteName", emailForSite.siteName);
            emailBody = emailBody.Replace("kStartDateMonthYear", emailForSite.changeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            emailBody = emailBody.Replace("kUnleadedPrice", GetPriceFormattedPriceForEmail(emailForSite.priceUnleaded));
            emailBody = emailBody.Replace("kLpgPrice", GetPriceFormattedPriceForEmail(emailForSite.priceLpg)); // (priceLpg / 10).ToString("###.0", CultureInfo.InvariantCulture));
            emailBody = emailBody.Replace("kDieselPrice", GetPriceFormattedPriceForEmail(emailForSite.priceDiesel)); // / 10).ToString("###.0", CultureInfo.InvariantCulture));

            return emailForSite.emailBody = emailBody;
        }

        private static string GetPriceFormattedPriceForEmail(decimal price)
        {
            return (price == 0) ? Constants.EmailPriceReplacementStringForZero : (price/10).ToString("####.0");
        }

        //TODO Log to Audit Table
        private void RecordEmailsSentToSites(string statusCode, string address, string reasonForFailure= "")
        {
            try
            {
                // Code to record email success/failure (TODO what level of audit details needed ??)
            }
            catch (Exception)
            {
                // suppress Audit errors for now
            }
        
        }

        /// <summary>
        /// Creates an SMTP Client based on AppSettings Keys
        /// </summary>
        /// <returns></returns>
        public static SmtpClient CreateSmtpClient()
        {
            // Localhost, Gmail, AWS
            var mailHostSelector = ConfigurationManager.AppSettings["mailHostSelector"];
            var client = new SmtpClient();
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
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(
                        userName: "akiaip5@gmail.com", 
                        password: "AmDoy02X");
                }
                break;
                case "AWS":
                {
                    //private const string smtpIAMUsername = "ses-smtp-user.20151202-103633"; // not needed 
                    client.Host = "email-smtp.eu-west-1.amazonaws.com";
                    client.Port = 587; //  25, 587, or 2587
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(
                        userName: "AKIAIP5MYCP3ETOHJ73A", 
                        password: "AmDoy02X/bZc5EBMh8AJiOsc6iyodxnN2K7F4epLl3Vt");
                }
                break;
            }
            return client;
        }

       
    }

    public class EmailSiteData
    {
        public string siteName {get; set;}
        public string emailBody { get; set; }
        public DateTime changeDate { get; set; }
        public decimal priceUnleaded { get; set; }
        public decimal priceLpg { get; set; }
        public decimal priceDiesel { get; set; }
        public bool atLeastOnePriceAvailable { get; set; }
    }

    
}

