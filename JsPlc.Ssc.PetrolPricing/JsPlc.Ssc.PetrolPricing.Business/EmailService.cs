﻿using System;
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
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.Dtos;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
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
        public async Task<ConcurrentDictionary<int, EmailSendLog>> SendEmailAsync(IEnumerable<Site> listSites,
            DateTime endTradeDate,
            string reportBackEmailAddr)
        {
            var sites = listSites as IList<Site> ?? listSites.ToList();
            // SERIES execution for now.
            foreach (Site site in sites)
            {
                var logEntry = new EmailSendLog();
                try
                {
                    var sendable = true;
                    // This Try can be a Task to run in parallel, 
                    // completion of each task can add entry to "sendLog"
                    _sendLog.TryAdd(site.Id, logEntry);

                    logEntry = logEntry.SetupLogEntry1(site.Id, endTradeDate, reportBackEmailAddr, DateTime.Now);

                    // EMAIL Task
                    //one email built per sites for multiple user in site email list
                    var emailBody = BuildEmailBody(site, endTradeDate);

                    var emailSubject = SettingsService.EmailSubject();
                    var emailFrom = SettingsService.EmailFrom();

                    using (var smtpClient = CreateSmtpClient())
                    {
                        var message = new MailMessage();

                        message.From = new MailAddress(emailFrom, emailFrom); // TODO - DisplayName same as fromEmail
                        message.Subject = site.SiteName + " - " + emailSubject;
                        message.Body = emailBody;
                        message.BodyEncoding = Encoding.ASCII;
                        message.IsBodyHtml = true;

                        var emailToSet = await GetEmailToAddresses(site);

                        var sendMode = String.IsNullOrEmpty(emailToSet.FixedEmailTo)
                            ? EmailSendMode.Live
                            : EmailSendMode.Test;

                        logEntry = logEntry.SetupLogEntry2(message, emailToSet);

                        if (sendMode == EmailSendMode.Test)
                        {
                            // In test mode, we still send the email to test email format etc., even if no site email addresses are set..
                            message.To.Add(emailToSet.FixedEmailTo);
                        }
                        else
                        {
                            emailToSet.ListOfEmailTo.ForEach(e => message.To.Add(e));
                        }

                        // For sites with no email set.. log a warning..in both Test and Live mode..
                        // Also in Live mode we "SKIP" sending email.. since we cant..
                        if (!emailToSet.ListOfEmailTo.Any()) // In Test mode, this would not be True ever !! 
                        {
                            logEntry.AddWarningMessageToLogEntry(
                                string.Format("Warning: No email(s) setup for siteId={0}, siteName={1}", site.Id,
                                    site.SiteName));
                            if (sendMode == EmailSendMode.Live)
                            {
                                sendable = false;
                                //continue; // In LIVE Mode, we cant send email as the To list is blank
                            }
                        }
                        if (String.IsNullOrEmpty(emailBody))
                        {
                            logEntry.EmailBody = emailBody;
                            logEntry.AddWarningMessageToLogEntry(
                                string.Format(
                                    "Warning: No email to send for siteId={0}, siteName={1}. Possibly no valid prices set to communicate for site.",
                                    site.Id,
                                    site.SiteName));
                            //continue; // In LIVE Mode, we shouldnt send email if body is blank  
                            sendable = false;
                        }

                        if (sendable)
                        {
#if !DEBUG
                            smtpClient.Send(message);
#endif
                            logEntry.SetSuccessful();
                        }
                    }
                }
                catch (Exception ex)
                {
                    EmailSendLog existingEntry;
                    if (!_sendLog.TryGetValue(site.Id, out existingEntry))
                    {
                        _sendLog.TryAdd(site.Id, logEntry);
                        existingEntry = logEntry;
                    }
                    existingEntry.AddErrorMessageToLogEntry(ex.Message);
                    // CURRENT Approach: Continue sending on failure..
                    // OR Break out.. Not sure yet.
                }
            } // end foreach

            return _sendLog;
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
            //if (String.IsNullOrEmpty(testEmailTo))

            // we want this list still, where to send will vbbe decided by message building
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
            //trade date prices
            var pricesWithDateCalcAsEndTradeDate = new List<SitePrice>();
            if (site.Prices != null && site.Prices.Any())
            {
                pricesWithDateCalcAsEndTradeDate = site.Prices.Where(x => x.DateOfCalc.Date.Equals(endTradeDate.Date)).ToList();
            }

            if (!pricesWithDateCalcAsEndTradeDate.Any())
            {
                emailForSite.atLeastOnePriceAvailable = false;
                return emailForSite;
            }

            //previous trade date prices
            var priceDifferenceFound = findPriceDifference(site, endTradeDate, pricesWithDateCalcAsEndTradeDate);

            //send email if we have got here
            var atLeastOne = false;

            //if price difference found
            //System will send an email if:
            //- there is price change for any fuel from previous trading date
            //- there is change in quantity of the fuels from previous trading date
            //- prices for previous trading date not found
            if (priceDifferenceFound)
            {
                foreach (SitePrice sp in pricesWithDateCalcAsEndTradeDate)
                {
                    if (sp.FuelTypeId == 2) // unleaded
                    {
                        emailForSite.priceUnleaded = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                        atLeastOne = true;
                    }
                    if (sp.FuelTypeId == 1) // Super unl.
                    {
                        emailForSite.priceSuper = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                        atLeastOne = true;
                    }
                    if (sp.FuelTypeId == 6) // diesel
                    {
                        emailForSite.priceDiesel = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
                        atLeastOne = true;
                    }

                }
            }
            emailForSite.atLeastOnePriceAvailable = atLeastOne;
            return emailForSite;
        }

        private static bool findPriceDifference(Site site, DateTime endTradeDate, List<SitePrice> pricesWithDateCalcAsEndTradeDate)
        {
            bool result = false;

            var pricesWithDateCalcAsPreviousEndTradeDate = new List<SitePrice>();

            if (site.Prices != null && site.Prices.Any())
            {
                //find previousTradeDate
                var previousTradeDate = site.Prices.Where(x => x.DateOfCalc.Date < endTradeDate.Date).OrderByDescending(x => x.DateOfCalc).FirstOrDefault();

                //send email if previous trading date not found
                if (previousTradeDate == null)
                {
                    result = true;
                }
                else
                {
                    pricesWithDateCalcAsPreviousEndTradeDate = site.Prices.Where(x => x.DateOfCalc.Date.Equals(previousTradeDate.DateOfCalc)).ToList();
                }
            }

            //send email - if no prices found for previous trade date
            if (pricesWithDateCalcAsPreviousEndTradeDate.Any() == false)
            {
                result = true;
            }
            else
            {
                //send email - if quantity of trading fuels has changed.
                if (pricesWithDateCalcAsPreviousEndTradeDate.Count != pricesWithDateCalcAsEndTradeDate.Count)
                {
                    result = true;
                }
                else
                {
                    foreach (var priceWithDateCalcAsEndTradeDate in pricesWithDateCalcAsEndTradeDate)
                    {
                        foreach (var priceWithDateCalcAsPreviousEndTradeDate in pricesWithDateCalcAsPreviousEndTradeDate)
                        {
                            //find matching fuel
                            if (priceWithDateCalcAsEndTradeDate.FuelTypeId == priceWithDateCalcAsPreviousEndTradeDate.FuelTypeId)
                            {
                                //previous trading date price
                                var previousTradingDatePrice = priceWithDateCalcAsPreviousEndTradeDate.OverriddenPrice == 0
                                    ? priceWithDateCalcAsPreviousEndTradeDate.SuggestedPrice
                                    : priceWithDateCalcAsPreviousEndTradeDate.OverriddenPrice;

                                //current trading date price
                                var currentTradingDatePrice = priceWithDateCalcAsEndTradeDate.OverriddenPrice == 0
                                    ? priceWithDateCalcAsEndTradeDate.SuggestedPrice
                                    : priceWithDateCalcAsEndTradeDate.OverriddenPrice;

                                //compare prices
                                if (previousTradingDatePrice - currentTradingDatePrice != 0)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }

                        //if price differece found don't need to go further
                        if (result)
                            break;
                    }
                }
            }
            return result;
        }

        private static string EmailReplaceTemplateKeys(EmailSiteData emailForSite)
        {
            string emailBody = emailForSite.emailBody;

            emailBody = emailBody.Replace("kSiteName", emailForSite.siteName);
            emailBody = emailBody.Replace("kStartDateMonthYear", emailForSite.changeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            emailBody = emailBody.Replace("kUnleadedPrice", GetPriceFormattedPriceForEmail(emailForSite.priceUnleaded));
            emailBody = emailBody.Replace("kSuperPrice", GetPriceFormattedPriceForEmail(emailForSite.priceSuper)); // (priceSuper / 10).ToString("###.0", CultureInfo.InvariantCulture));
            emailBody = emailBody.Replace("kDieselPrice", GetPriceFormattedPriceForEmail(emailForSite.priceDiesel)); // / 10).ToString("###.0", CultureInfo.InvariantCulture));

            return emailForSite.emailBody = emailBody;
        }

        private static string GetPriceFormattedPriceForEmail(decimal price)
        {
            return (price == 0) ? Constants.EmailPriceReplacementStringForZero : (price / 10).ToString("####.0");
        }

        // optional 
        static void smtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // optional: to be implemented 
        }


        public async Task<List<EmailSendLog>> SaveEmailLogToRepositoryAsync(List<EmailSendLog> logEntries)
        {
            List<EmailSendLog> savedEntries = null;
            savedEntries = await _db.LogEmailSendLog(logEntries);
            return savedEntries;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="forDate"></param>
        /// <returns></returns>
        public async Task<List<EmailSendLog>> GetEmailSendLog(int siteId, DateTime? forDate)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            var retval = await _db.GetEmailSendLog(siteId, forDate.Value);
            return retval;
        }
    }

    public class EmailSiteData
    {
        public string siteName { get; set; }
        public string emailBody { get; set; }
        public DateTime changeDate { get; set; }
        public decimal priceUnleaded { get; set; }
        public decimal priceSuper { get; set; }
        public decimal priceDiesel { get; set; }
        public bool atLeastOnePriceAvailable { get; set; }
    }
}

