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

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class EmailService : IEmailService
	{
		private readonly ConcurrentDictionary<int, EmailSendLog> _sendLog =
			new ConcurrentDictionary<int, EmailSendLog>();

		protected readonly IPetrolPricingRepository _db;

		protected readonly IFactory _factory;

		protected readonly IAppSettings _appSettings;

		public EmailService(IPetrolPricingRepository db,
			IAppSettings appSettings,
			IFactory factory)
		{
			_db = db;
			_factory = factory;
			_appSettings = appSettings;
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
			IEnumerable<Site> listSites,
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

					var emailSubject = _appSettings.EmailSubject;
					var emailFrom = _appSettings.EmailFrom;

					using (var smtpClient = createSmtpClient())
					{
						var message = new MailMessage();

						message.From = new MailAddress(emailFrom, emailFrom); // TODO - DisplayName same as fromEmail
						message.Subject = site.SiteName + " - " + emailSubject;
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
								string.Format("Warning: No email(s) setup for siteId={0}, siteName={1}. ", site.Id,
									site.SiteName));
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
									site.Id,
									site.SiteName));
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

		public static string BuildEmailBody(Site site, DateTime endTradeDate)
		{
			var emailForSite = new EmailSiteData();

			emailForSite = emailSetValues(site, emailForSite, endTradeDate);

			if (emailForSite == null || !emailForSite.AtLeastOnePriceAvailable)
			{
				return string.Empty;
			}

			emailForSite.EmailBody = emailGetLayout();

			emailForSite.EmailBody = emailReplaceTemplateKeys(emailForSite);

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

			//Using an SMTP client with the specified host name and port.
			using (var client = createSmtpClient())
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
		/// Creates an SMTP Client based on AppSettings Keys
		/// </summary>
		/// <returns></returns>
		private ISmtpClient createSmtpClient()
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

		//Below helper methods used by build email. 
		private static string emailGetLayout()
		{
			var filePathAndName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin/Templates/EmailTemplate.html");

			StringBuilder sb = new StringBuilder();

			using (StreamReader sr = new StreamReader(filePathAndName))
			{
				sb.Append(sr.ReadToEnd());
			}

			return sb.ToString();
		}

		/// <summary>
		/// Get SitePrice data for email, returns null if not found
		/// </summary>
		/// <param name="site"></param>
		/// <param name="emailForSite"></param>
		/// <param name="endTradeDate"></param>
		/// <returns></returns>
		private static EmailSiteData emailSetValues(Site site, EmailSiteData emailForSite, DateTime endTradeDate)
		{
			emailForSite.SiteName = site.SiteName;
			emailForSite.ChangeDate = endTradeDate;
			
			//trade date prices
			var pricesWithDateCalcAsEndTradeDate = new List<SitePrice>();

			if (site.Prices != null && site.Prices.Any())
			{
				pricesWithDateCalcAsEndTradeDate = site.Prices.Where(x => x.DateOfCalc.Date.Equals(endTradeDate.Date)).ToList();
			}

			if (false == pricesWithDateCalcAsEndTradeDate.Any())
			{
				emailForSite.AtLeastOnePriceAvailable = false;
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
						emailForSite.PriceUnleaded = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
						atLeastOne = true;
					}
					if (sp.FuelTypeId == 1) // Super unl.
					{
						emailForSite.PriceSuper = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
						atLeastOne = true;
					}
					if (sp.FuelTypeId == 6) // diesel
					{
						emailForSite.PriceDiesel = (sp.OverriddenPrice == 0) ? sp.SuggestedPrice : sp.OverriddenPrice;
						atLeastOne = true;
					}

				}
			}
			emailForSite.AtLeastOnePriceAvailable = atLeastOne;
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

						//if at least one price differece found don't need to go further
						if (result)
							break;
					}
				}
			}
			return result;
		}

		private static string emailReplaceTemplateKeys(EmailSiteData emailForSite)
		{
			var emailBody = new StringBuilder(emailForSite.EmailBody);

			emailBody.Replace("{SiteName}", emailForSite.SiteName);
			emailBody.Replace("{StartDateMonthYear}", emailForSite.ChangeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
			emailBody.Replace("{UnleadedPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceUnleaded));
			emailBody.Replace("{SuperPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceSuper));
			emailBody.Replace("{DieselPrice}", getPriceFormattedPriceForEmail(emailForSite.PriceDiesel)); 

			return emailForSite.EmailBody = emailBody.ToString();
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
		private Task<EmailToSet> getEmailToAddresses(Site site)
		{
			EmailToSet emailToSet = new EmailToSet();

			var testEmailTo = _appSettings.FixedEmailTo;
			
			emailToSet.FixedEmailTo = testEmailTo;

			emailToSet.ListOfEmailTo = new List<string>();

			emailToSet.ListOfEmailTo.AddRange(site.Emails.Where(x => !string.IsNullOrEmpty(x.EmailAddress))
					.Select(email => email.EmailAddress));
			emailToSet.CommaSeprListOfEmailTo = String.Join(",", emailToSet.ListOfEmailTo);
			
			return Task.FromResult(emailToSet);
		}

		#endregion

	}
}

