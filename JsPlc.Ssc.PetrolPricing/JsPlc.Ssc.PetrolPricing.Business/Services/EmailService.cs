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
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.Dtos;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Repository;
using MoreLinq;
using System.Diagnostics;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;
using JsPlc.Ssc.PetrolPricing.Business.Services;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class EmailService : IEmailService
	{
        private static Dictionary<string, string> _importanceInlineStyles = new Dictionary<string, string>()
        {
            {"importance-normal", "" },
            {"importance-info", "font-weight: bold; color: green;" },
            {"importance-warning", "font-weight: bold; color: orange;" },
            {"importance-danger", "font-weight: bold; color: red;" }
        };


		private readonly ConcurrentDictionary<int, EmailSendLog> _sendLog =
			new ConcurrentDictionary<int, EmailSendLog>();

		protected readonly IPetrolPricingRepository _db;

		protected readonly IFactory _factory;

		protected readonly IAppSettings _appSettings;

	    protected readonly ILogger _logger;

        private ISystemSettingsService _systemSettingsService;


		public EmailService(IPetrolPricingRepository db,
			IAppSettings appSettings,
			IFactory factory,
            ISystemSettingsService systemSettingsService)
		{
			_db = db;
			_factory = factory;
			_appSettings = appSettings;
            _systemSettingsService = systemSettingsService;
		    _logger = new PetrolPricingLogger();
		}

		/// <summary>
		/// Send email to users listed in each site 
		/// Logs report to DB AND sends report to reportBackEmailAddr
		/// </summary>
		/// <param name="listSites">One or more sites selected by caller method</param>
		/// <param name="endTradeDate">Usually today's date</param>
		/// <param name="reportBackEmailAddr">Emails the send log to reportBackEmailAddr(LoginUser)</param>
		/// <returns></returns>
		public async Task<ConcurrentDictionary<int, EmailSendLog>> SendEmailAsync(
            EmailTemplate emailTemplate,
            List<SitePriceViewModel> listSites,
			DateTime endTradeDate,
			string reportBackEmailAddr)
		{
            var sites = listSites as IList<SitePriceViewModel> ?? listSites.ToList();
			// SERIES execution for now.
            foreach (SitePriceViewModel site in sites)
			{
				var logEntry = new EmailSendLog();
				try
				{
					var sendable = true;
					// This Try can be a Task to run in parallel, 
					// completion of each task can add entry to "sendLog"
					_sendLog.TryAdd(site.SiteId, logEntry);

                    logEntry = logEntry.SetupLogEntry1(site.SiteId, endTradeDate, reportBackEmailAddr, DateTime.Now);

					// EMAIL Task
					//one email built per sites for multiple user in site email list
					var emailBody = BuildEmailBody(emailTemplate, site, endTradeDate);

                    var emailSubject = ReplaceEmailSubjectLineTokens(emailTemplate, site, endTradeDate);

                    var emailFrom = _appSettings.EmailFrom;

                    var emailSettings = new EmailServiceSettings(_appSettings, _factory);

					using (var smtpClient = emailSettings.CreateSmtpClient())
					{
						var message = new MailMessage();

						message.From = new MailAddress(emailFrom, emailFrom); // TODO - DisplayName same as fromEmail
                        message.Subject = emailSubject;
                        message.Body = emailBody;
						message.BodyEncoding = Encoding.ASCII;
						message.IsBodyHtml = true;

						var emailToSet = await getEmailToAddresses(site);

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
								string.Format("Warning: No email(s) setup for siteId={0}, siteName={1}. ", site.SiteId,
                                    site.StoreName));
							if (sendMode == EmailSendMode.Live)
							{
								sendable = false;
								//continue; // In LIVE Mode, we cant send email as the To list is blank
							}
						}
						//emailBody will be empty if there are not changes found for the site
						if (String.IsNullOrEmpty(emailBody))
						{
							logEntry.EmailBody = emailBody;
							logEntry.AddWarningMessageToLogEntry(
								string.Format(
									"Warning: No email to send for siteId={0}, siteName={1}. Possibly no valid prices set to communicate for site.",
									site.SiteId,
                                    site.StoreName));
							//continue; // In LIVE Mode, we shouldnt send email if body is blank  
							sendable = false;
						}

						if (sendable)
						{
                            smtpClient.Send(message);
							logEntry.SetSuccessful();
						}
					}
				}
				catch (Exception ex)
				{
                    _logger.Error(ex);
					EmailSendLog existingEntry;
                    if (!_sendLog.TryGetValue(site.SiteId, out existingEntry))
					{
                        _sendLog.TryAdd(site.SiteId, logEntry);
						existingEntry = logEntry;
					}
					existingEntry.AddErrorMessageToLogEntry(ex.Message);
					// CURRENT Approach: Continue sending on failure..
					// OR Break out.. Not sure yet.
				}
			} // end foreach

			return _sendLog;
		}

        public string BuildEmailBody(EmailTemplate emailTemplate, SitePriceViewModel site, DateTime endTradeDate)
		{
			var emailForSite = new EmailSiteData();

			emailForSite = emailSetValues(site, emailForSite, endTradeDate);

			if (emailForSite == null || !emailForSite.AtLeastOnePriceAvailable)
			{
				return string.Empty;
			}

            emailForSite.EmailBody = emailReplaceTemplateKeys(emailTemplate, emailForSite);

            emailForSite.EmailBody = ApplyEmailStyles(emailForSite.EmailBody);

            return emailForSite.EmailBody;
		}

		public async Task<List<EmailSendLog>> SaveEmailLogToRepositoryAsync(List<EmailSendLog> logEntries)
		{
			List<EmailSendLog> savedEntries = null;
			savedEntries = await _db.LogEmailSendLog(logEntries);
			return savedEntries;
		}

		public string SendTestEmail()
		{
			string result;

            var emailSettings = new EmailServiceSettings(_appSettings, _factory);

			//Using an SMTP client with the specified host name and port.
			using (var client = emailSettings.CreateSmtpClient())
			{
				string mailFrom = _appSettings.EmailFrom; // "akiaip5@gmail.com";

				const string mailTo = "andrey.shihov@sainsburys.co.uk"; //"akiaip5@gmail.com";
				const string mailSubject = "Hello, Test Email from AWS";
				const string mailBody = "<h1>Hello, This is a <span syle='color: red'>Test Email from Smtp</span> from C# code</h1>";

				// Send the email. 
				try
				{
					Debug.WriteLine("TestSendMail: Attempting to send an email through the Amazon SES SMTP interface...");

					var mailMsg = new MailMessage(mailFrom, mailTo)
					{
						IsBodyHtml = true,
						Subject = mailSubject,
						Body = mailBody,
					};

					client.Send(mailMsg);

					result = "TestSendMail: Email sent!";
					Debug.WriteLine("TestSendMail: Email sent!");
				}
				catch (Exception ex)
				{
                    _logger.Error(ex);
					Debug.WriteLine("TestSendMail: The email was not sent.");
					var innerMsg = (ex.InnerException != null) ? ex.InnerException.Message : "";
					result = "TestSendMail: Error message: " + ex.Message + innerMsg;
					Debug.WriteLine(result);
				}
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="forDate"></param>
		/// <returns></returns>
		public async Task<List<EmailSendLog>> GetEmailSendLog(int siteId, DateTime? forDate)
		{
			if (!forDate.HasValue)
			{
				forDate = DateTime.Now;
			}

			var retval = await _db.GetEmailSendLog(siteId, forDate.Value);

			return retval;
		}

        #region Private Methods

		/// <summary>
		/// Get SitePrice data for email, returns null if not found
		/// </summary>
		/// <param name="site"></param>
		/// <param name="emailForSite"></param>
		/// <param name="endTradeDate"></param>
		/// <returns></returns>
        private EmailSiteData emailSetValues(SitePriceViewModel site, EmailSiteData emailForSite, DateTime endTradeDate)
		{
			emailForSite.SiteName = site.StoreName;
			emailForSite.ChangeDate = endTradeDate;
			
			if (site.FuelPrices == null || site.FuelPrices.Count==0)
			{
				emailForSite.AtLeastOnePriceAvailable = false;
				return emailForSite;
			}


            var pricingSettings = _systemSettingsService.GetSitePricingSettings();

            //previous trade date prices
            var priceDifferenceFound = findPriceDifference(site, pricingSettings);

			//send email if we have got here
			var atLeastOne = false;

			//if price difference found
			//System will send an email if:
			//- there is price change for any fuel from previous trading date
			//- there is change in quantity of the fuels from previous trading date
			//- prices for previous trading date not found
			if (priceDifferenceFound)
			{
                foreach (var fuelPrice in site.FuelPrices)
				{
                    if (fuelPrice.FuelTypeId == 2) // unleaded
					{
                        var overridePrice = 0;
                        if (fuelPrice.OverridePrice.HasValue)
                        {
                            overridePrice = fuelPrice.OverridePrice.Value;
                        }
                        emailForSite.PriceUnleaded = (overridePrice == 0) ? fuelPrice.AutoPrice.Value : overridePrice;
						atLeastOne = true;
					}
                    if (fuelPrice.FuelTypeId == 1) // Super unl.
					{
					    var overridePrice = 0;
                        if (fuelPrice.OverridePrice.HasValue)
                        {
                            overridePrice = fuelPrice.OverridePrice.Value;
                        }
                        emailForSite.PriceSuper = (overridePrice == 0) ? fuelPrice.AutoPrice.Value : overridePrice;
						atLeastOne = true;
					}
                    if (fuelPrice.FuelTypeId == 6) // diesel
					{
						var overridePrice = 0;
                        if (fuelPrice.OverridePrice.HasValue)
                        {
                            overridePrice = fuelPrice.OverridePrice.Value;
                        }
                        emailForSite.PriceDiesel = (overridePrice == 0) ? fuelPrice.AutoPrice.Value : overridePrice;
						atLeastOne = true;
					}

				}
			}
			emailForSite.AtLeastOnePriceAvailable = atLeastOne;
			return emailForSite;
		}

        private static bool findPriceDifference(SitePriceViewModel site, SitePricingSettings pricingSettings)
		{
			bool result = false;
            foreach (var fuelprice in site.FuelPrices)
            {
                //previous trading date price
                var previousTradingDatePrice = fuelprice.TodayPrice;

                //current trading date price
                var currentTradingDatePrice = fuelprice.OverridePrice == 0
                ? fuelprice.AutoPrice
                : fuelprice.OverridePrice;

                if (previousTradingDatePrice > 0 && currentTradingDatePrice > 0)
                {
                    var diff = previousTradingDatePrice - currentTradingDatePrice;

                    //compare prices
                    if (diff.HasValue) {
                        var changePerLitre = (double)diff.Value / 10;
                        if (Math.Abs(changePerLitre) >= pricingSettings.PriceChangeVarianceThreshold)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                //if at least one price differece found don't need to go further
                if (result)
                    break;
            }
			return result;
		}

        private static string ReplaceEmailSubjectLineTokens(EmailTemplate emailTemplate, SitePriceViewModel emailForSite, DateTime changeDate)
        {
            var subjectline = emailTemplate.SubjectLine;
            subjectline = subjectline.Replace("{SiteName}", emailForSite.StoreName);
            subjectline =  subjectline.Replace("{DayMonthYear}", changeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            return subjectline;
        }

		private static string emailReplaceTemplateKeys(EmailTemplate emailTemplate, EmailSiteData emailForSite)
		{
            var emailBody = new StringBuilder(emailTemplate.EmailBody);

			emailBody.Replace("{SiteName}", emailForSite.SiteName);
			emailBody.Replace("{DayMonthYear}", emailForSite.ChangeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
			emailBody.Replace("{UnleadedPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceUnleaded));
			emailBody.Replace("{SuperPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceSuper));
			emailBody.Replace("{DieselPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceDiesel)); 

			return emailForSite.EmailBody = emailBody.ToString();
		}

        private static string ApplyEmailStyles(string emailBody)
        {
            foreach(var item in _importanceInlineStyles)
            {
                var oldText = String.Format("class=\"{0}\"", item.Key);
                var newText = String.Format("style=\"{0}\"", item.Value);
                emailBody = emailBody.Replace(oldText, newText);
            }
            return emailBody;
        }

        private static string getPriceFormattedPriceForEmail(decimal price)
		{
			return (price == 0) ? Constants.EmailPriceReplacementStringForZero : (price / 10).ToString("####.0");
		}

		/// <summary>
		/// Builds a list of email addresses including the FixedEmail and a list of emails to send to
		/// </summary>
		/// <param name="site"></param>
		/// <returns></returns>
        private Task<EmailToSet> getEmailToAddresses(SitePriceViewModel site)
		{
			EmailToSet emailToSet = new EmailToSet();

			var testEmailTo = _appSettings.FixedEmailTo;
			
			emailToSet.FixedEmailTo = testEmailTo;

			emailToSet.ListOfEmailTo = new List<string>();

            emailToSet.ListOfEmailTo = site.Emails;
			emailToSet.CommaSeprListOfEmailTo = String.Join(",", emailToSet.ListOfEmailTo);
			
			return Task.FromResult(emailToSet);
		}

		#endregion
    }
}

