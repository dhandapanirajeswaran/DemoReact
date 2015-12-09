using System;
using System.Collections.Generic;
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

        //send email to users providied in EmailService list
        public bool SendEmail(IEnumerable<Site> listSites, DateTime endTradeDate)
        {
            try
            {
                foreach (Site site in listSites)
                {
                    //one email built per sites for multiple user in site email list
                    string emailBody = BuildEmail(site, endTradeDate);

                    string emailSubject = ConfigurationManager.AppSettings["emailSubject"];
                    string emailFrom = ConfigurationManager.AppSettings["emailFrom"];

                    foreach (SiteEmail email in site.Emails)
                    {
                        using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["smtpServer"], int.Parse(ConfigurationManager.AppSettings["smtpPort"])))
                        {
                            try
                            {
                                MailMessage message = new MailMessage();
                                message.From = new MailAddress(email.EmailAddress, emailFrom);
                                message.Subject = site.SiteName + " - " + emailSubject;
                                message.Body = emailBody;
                                message.BodyEncoding = Encoding.ASCII;
                                message.IsBodyHtml = true;

                                smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["smtpUserName"], ConfigurationManager.AppSettings["smtpPassword"]);
                                smtp.EnableSsl = true;
                                smtp.Send(message);
                            }
                            catch 
                            {
                                RecordEmailsSentToSites();//FAIL - Not Send
                            }
                        }

                        RecordEmailsSentToSites();//Succsess - Sent 
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private string BuildEmail(Site site, DateTime endTradeDate)
        {
            EmailSiteData emailForSite = new EmailSiteData();

            emailForSite = EmailSetValues(site, emailForSite, endTradeDate);
            emailForSite.emailBody = EmailGetLayout();
            emailForSite.emailBody = EmailReplaceTemplateKeys(emailForSite);

            return emailForSite.emailBody;
        }

        //Below helper methods used by build email. 
        private string EmailGetLayout()
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
            sb.Append("<tr><td>Super (if applicable)</td><td>kSuperPrice</td></tr>");
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

        private EmailSiteData EmailSetValues(Site site, EmailSiteData emailForSite, DateTime endTradeDate)
        {
            emailForSite.siteName = site.SiteName;
            emailForSite.changeDate = endTradeDate;
           
             foreach(SitePrice sp in site.Prices)
             {
                if(sp.FuelType.FuelTypeName == "Unleaded" )
                {
                    emailForSite.priceUnleaded = sp.SuggestedPrice;
                    
                }
                if(sp.FuelType.FuelTypeName == "Super Unleaded")
                {
                     emailForSite.priceSuper = sp.SuggestedPrice;
                }
                if(sp.FuelType.FuelTypeName == "Diesel")
                {
                    emailForSite.priceDiesel = sp.SuggestedPrice;
                }
             }

             return emailForSite;
        }

        private string EmailReplaceTemplateKeys(EmailSiteData EmailForSite)
        {

            string xxx = "";//changeDate.DayOfWeek.ToString() + " " + changeDate.Month.ToString(), + " " + changeDate.Year.ToString();

            string emailBody = EmailForSite.emailBody;

            emailBody.Replace("kSiteName", EmailForSite.siteName);
            emailBody.Replace("kStartDateMonthYear", EmailForSite.changeDate.ToString());
            emailBody.Replace("kUnleadedPrice", EmailForSite.priceUnleaded.ToString());
            emailBody.Replace("kSuperPrice", EmailForSite.priceSuper.ToString());
            emailBody.Replace("kDieselPrice", EmailForSite.priceDiesel.ToString());

            return EmailForSite.emailBody = emailBody;
        }

        //TODO Log to Audit Table
        private void RecordEmailsSentToSites()
        {
        
        }

       
    }

    public class EmailSiteData
    {
        public string siteName {get; set;}
        public string emailBody { get; set; }
        public DateTime changeDate { get; set; }
        public decimal priceUnleaded { get; set; }
        public decimal priceSuper { get; set; }
        public decimal priceDiesel { get; set; }
    }

    
}

