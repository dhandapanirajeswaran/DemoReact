using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System.Net;

using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using MoreLinq;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class EmailService : BaseService, IDisposable
    {
        private readonly ConcurrentDictionary<int, EmailSendLog> _sendLog = 
            new ConcurrentDictionary<int, EmailSendLog>();

        public void Dispose()
        {
            // do nothing for now
        }


        /// <summary>
        /// Send email to users listed in each site 
        /// Logs report to DB AND sends report to reportBackEmailAddr
        /// </summary>
        /// <param name="listSites">One or more sites selected by caller method</param>
        /// <param name="endTradeDate">Usually today's date</param>
        /// <param name="reportBackEmailAddr">Emails the send log to reportBackEmailAddr(LoginUser)</param>
        /// <returns></returns>
        public async Task<bool> SendEmailAsync(IEnumerable<Site> listSites, DateTime endTradeDate, 
            string reportBackEmailAddr)
        {
            var sites = listSites as IList<Site> ?? listSites.ToList();
            try
            {
                foreach (Site site in sites)
                {
                    try
                    {
                        // This Try can be a Task to run in parallel, 
                        // completion of each task can add entry to "sendLog"

                        //one email built per sites for multiple user in site email list
                        var emailBody = BuildEmailBody(site, endTradeDate);
                        if (String.IsNullOrEmpty(emailBody)) continue;

                        var emailSubject = SettingsService.EmailSubject();
                        var emailFrom = SettingsService.EmailFrom();

                        using (var smtpClient = CreateSmtpClient())
                        {
                            var emailToSet = await GetEmailToAddresses(site); 

                            var message = new MailMessage();
                            message.From = new MailAddress(emailFrom, emailFrom); // TODO - DisplayName same as fromEmail

                            message.Subject = site.SiteName + " - " + emailSubject;

                            if (String.IsNullOrEmpty(emailToSet.FixedEmailTo))
                            {
                                emailToSet.ListOfEmailTo.ForEach(e => message.To.Add(e));                               
                            }
                            else
                            {
                                message.To.Add(emailToSet.FixedEmailTo);
                            }

                            message.Body = emailBody;
                            message.BodyEncoding = Encoding.ASCII;
                            message.IsBodyHtml = true;

                            var logEntry = CreateLogEntry(site.Id, 
                                message, 
                                emailToSet, endTradeDate, 
                                reportBackEmailAddr, 
                                DateTime.Now, "");

                            _sendLog.TryAdd(site.Id, logEntry);

                            smtpClient.SendCompleted += SmtpClientSendCompleted;
                            smtpClient.SendAsync(message, logEntry); // can throw exception return type is Task

                            // OR use SendMailAsync
                            //var userToken = message;
                            //smtpClient.SendAsync(message, userToken); 
                        }
                    }
                    catch(Exception ex)
                    {
                        RecordEmailsSentToSite(site.Id, ex.Message); //FAIL - Not Send
                        // Continue on failure..
                    }
                    // maybe not needed, Success - Sent since we have completed handler..
                    //RecordEmailsSentToSites("Success", site.SiteName); 
                } // end foreach
            }
            catch(Exception ex) // General failure, report back to User UI.. 
            {
                RecordEmailGeneralFailure(sites, endTradeDate, reportBackEmailAddr, ex.Message); //FAIL - Could Not Send
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a log entry object
        /// We can add errorMessage and status to it on SendCompleted
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="message"></param>
        /// <param name="emailToSet"></param>
        /// <param name="endTradeDate"></param>
        /// <param name="loginUser"></param>
        /// <param name="sendDateTime"></param>
        /// <param name="errMessage"></param>
        /// <returns></returns>
        private EmailSendLog CreateLogEntry(int siteId, MailMessage message, EmailToSet emailToSet, 
            DateTime endTradeDate, string loginUser, DateTime sendDateTime, string errMessage)
        {
            return new EmailSendLog
            {
                EmailBody = message.Body,
                EmailFrom = message.From.Address,
                EmailSubject = message.Subject,
                FixedEmailTo = emailToSet.FixedEmailTo,
                ListOfEmailTo = emailToSet.CommaSeprListOfEmailTo,
                EndTradeDate = endTradeDate,
                IsTest = !String.IsNullOrEmpty(emailToSet.FixedEmailTo),
                LoginUser = loginUser,
                SendDate = sendDateTime,
                SiteId = siteId,
                Status = String.IsNullOrEmpty(errMessage) ? 0 : 1 // when do we set warning 
            };
        }

        /// <summary>
        /// Builds a list of email addresses including the FixedEmail and a list of emails to send to
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public Task<EmailToSet> GetEmailToAddresses(Site site)
        {
            EmailToSet emailToSet = new EmailToSet();

            var testEmailTo = SettingsService.FixedEmailTo();
            emailToSet.FixedEmailTo = testEmailTo;

            emailToSet.ListOfEmailTo = new List<string>();
            if (String.IsNullOrEmpty(testEmailTo))
            {
                emailToSet.ListOfEmailTo.AddRange(site.Emails.Where(x => !string.IsNullOrEmpty(x.EmailAddress))
                    .Select(email => email.EmailAddress));
            }
            emailToSet.CommaSeprListOfEmailTo = String.Join(",", emailToSet.ListOfEmailTo);
            return Task.FromResult(emailToSet);
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

        // optional 
        static void smtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // optional: to be implemented 
        }

        private void SmtpClientSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO Commit each Log entry to DB here..
            // TODO Also add each entry to a list so we can track completions..

            var smtpClient = (SmtpClient) sender;
            var logEntry = (EmailSendLog) e.UserState; // we might use full msg here
            smtpClient.SendCompleted -= SmtpClientSendCompleted; // remove handler from smtpClient

            // Since the logEntry is already in the _sendLog, we simply update it

            //EmailSendLog existingEntry;
            //_sendLog.TryGetValue(logEntry.SiteId, out existingEntry);

            if (e.Error != null)
            {
                logEntry.ErrorMessage = e.Error.Message;
                logEntry.Status = 1;
                //tracer.ErrorEx(
                //    e.Error,
                //    string.Format("Message sending for \"{0}\" failed.", userAsyncState.EmailMessageInfo.RecipientName)
                //    );
            }
            else
            {
                logEntry.ErrorMessage = "";
                logEntry.Status = 0;
            }

            // Cleaning up resources
            //.....
            smtpClient.Dispose();
        }

        private void RecordEmailGeneralFailure(IEnumerable<Site> sites, 
            DateTime endTradeDate, string reportBackEmailAddr, string failureMessage)
        {
            var paraSites= new ConcurrentBag<string>();
            sites.AsParallel().ForEach(site =>
            {
                var s = site;
                if (s.CatNo != null) paraSites.Add(s.CatNo.Value.ToString(CultureInfo.InvariantCulture));
            });
            var commaSeprSites = String.Join(",", paraSites.ToArray());

            try
            {
                var logEntries = new List<EmailSendLog>
                {
                    new EmailSendLog()
                    {
                        CommaSeprSiteCatIds = commaSeprSites,
                        ErrorMessage = failureMessage,
                        Status = 1, 
                        
                        EndTradeDate = endTradeDate,
                        SiteId = -1,
                        LoginUser = reportBackEmailAddr
                    }
                };
                _sendLog.TryAdd(-1, logEntries.First());
            }
            catch
            {
                // suppress Audit errors for now
            }
        }

        private async Task<List<EmailSendLog>> SaveEmailLogToRepository(List<EmailSendLog> logEntries)
        {
            List<EmailSendLog> savedEntries = null;
            savedEntries = await _db.LogEmailSendLog(logEntries);
            return savedEntries;
        }

        //TODO Log to Audit Table
        private void RecordEmailsSentToSite(int siteId, string reasonForFailure= "")
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
            var mailHostSelector = SettingsService.MailHostSelector();
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

    public class EmailToSet
    {
        public string FixedEmailTo { get; set; }
        public List<string> ListOfEmailTo { get; set; }
        public string CommaSeprListOfEmailTo { get; set; }
    }

}

