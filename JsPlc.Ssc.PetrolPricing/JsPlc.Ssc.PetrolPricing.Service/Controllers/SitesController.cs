using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using WebGrease.Css.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class SitesController : BaseController
    {
        public SitesController() { }

        public SitesController(SiteService siteService) : base(null, siteService, null) { }

        [System.Web.Http.HttpGet]
        //[Route("api/site/{id}")] // Not needed but works
        public IHttpActionResult Get([FromUri]int id)
        {
            var site = _siteService.GetSite(id);

            if (site == null)
                return NotFound();

            return Ok(site.ToSiteViewModel());
        }

        [System.Web.Http.HttpGet]
        //[Route("api/sites")]
        public IHttpActionResult Get()
        {
            var sites = _siteService.GetJsSites();
            var sitesList = sites as Site[] ?? sites.ToArray();
            if (sites == null || !sitesList.Any())
                return NotFound();
            List<SiteViewModel> sitesVm = sitesList.ToList().ToSiteViewModelList();

            return Ok(sitesVm);
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/brands")]
        public IHttpActionResult GetBrands()
        {
            var sites = _siteService.GetSites().Where(s => string.IsNullOrWhiteSpace(s.Brand) == false).Select(s => s.Brand).Distinct().OrderBy(x => x).ToList();
            if (!sites.Any())
                return NotFound();
            return Ok(sites);
        }

        /// <summary>
        /// Create new site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost] 
        public IHttpActionResult Post(SiteViewModel site)
        {
            if (site == null)
            {
                return BadRequest("Invalid passed data: site");
            }

            try
            {
                using (var ss = _siteService)
                {
                    if (ss.ExistsSite(site.SiteName, site.CatNo))
                    {
                        return BadRequest("Site with that name already exists. Please try again.");
                    }
                    var su = ss.NewSite(site.ToSite());
                    return Ok(su.ToSiteViewModel());
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }


        [System.Web.Http.HttpPut] // Edit new site
        public async Task<IHttpActionResult> Update(SiteViewModel site)
        {
            if (site == null)
            {
                return BadRequest("Invalid passed data: site");
            }

            try
            {
                using (var ss = _siteService)
                {
                    ss.UpdateSite(site.ToSite());
                    return Ok(site);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("api/SaveOverridePrices/")]
        public async Task<IHttpActionResult> PutOverridePrices(List<OverridePricePostViewModel> pricesToSave)
        {
            try
            {
                int rows;
                using (var ps = new PriceService())
                {
                    rows = await ps.SaveOverridePricesAsync(pricesToSave);
                }
                return Ok(rows);
            }
            catch (Exception ex) // format the exception to report back to Client
            {
                return new ExceptionResult(ex, this);
                //throw new HttpResponseException(new HttpResponseMessage {
                //    ReasonPhrase = ex.Message, StatusCode = HttpStatusCode.BadRequest, 
                //    Content = new StringContent(ex.Message),
                //});
            }
        }

        /// <summary>
        ///  Not used yet, churns out a lot of data
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/SiteDetails/")]
        public IHttpActionResult GetSitesWithEmailsAndPrices()
        {
            var sites = _siteService.GetSitesWithEmailsAndPrices();
            return Ok(sites);
        }

        /// <summary>
        /// Gets a list of SitePriceViewModel for SitePricing tab main data
        /// Test Url: api/SitePrices?forDate=2015-11-30&amp;siteId=1 
        /// or api/SitePrices
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="storeName"></param>
        /// <param name="catNo"></param>
        /// <param name="storeNo"></param>
        /// <param name="storeTown"></param>
        /// <param name="siteId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/SitePrices")]
        public IHttpActionResult GetSitesWithPrices(
            [FromUri] DateTime? forDate = null,
            [FromUri]string storeName = "",
            [FromUri]int catNo = 0,
            [FromUri]int storeNo = 0,
            [FromUri]string storeTown = "",
            [FromUri]int siteId = 0, [FromUri]int pageNo = 1, [FromUri]int pageSize = Constants.PricePageSize)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            IEnumerable<SitePriceViewModel> siteWithPrices = _siteService.GetSitesWithPrices(forDate.Value, storeName, catNo, storeNo, storeTown, siteId, pageNo, pageSize);
            return Ok(siteWithPrices.ToList());
        }

        /// <summary>
        /// Gets a list of SitePriceViewModel for SitePricing tab collapsible data
        /// Test Url: api/CompetitorPrices?forDate=2015-12-17&amp;siteId=1 
        /// or api/SitePrices
        /// </summary>
        /// <param name="forDate">Optional - Date of Calc/Viewing</param>
        /// <param name="siteId">Optional - Specific SiteId or 0 for all Sites</param>
        /// <param name="pageNo">Optional - Viewing PageNo</param>
        /// <param name="pageSize">Optional - PageSize, set large value (e.g. 1000) to get all sites</param>
        /// <returns>List of SitePriceViewModel</returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/CompetitorPrices")]
        public IHttpActionResult GetCompetitorsWithPrices([FromUri] DateTime? forDate = null,
            [FromUri]int siteId = 0, [FromUri]int pageNo = 1, [FromUri]int pageSize = Constants.PricePageSize)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            IEnumerable<SitePriceViewModel> siteWithPrices = _siteService.GetCompetitorsWithPrices(forDate.Value, siteId, pageNo, pageSize);
            return Ok(siteWithPrices.ToList());
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/TestSendMail")]
        public IHttpActionResult TestSendMail()
        {
            string result;

            //Using an SMTP client with the specified host name and port.
            using (var client = EmailService.CreateSmtpClient())
            {
                string mailFrom = SettingsService.EmailFrom(); // "akiaip5@gmail.com";

                const string mailTo = "somesiteEmail@sainsburys.co.uk"; //"akiaip5@gmail.com";
                const string mailSubject = "Hello, Test Email from Gmail SMTP 587";
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
#if !DEBUG
                    client.Send(mailMsg);
#endif
                    //client.Send(mailFrom, mailTo, mailSubject, mailBody);
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
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId">0 to send for all sites, otherwise specific siteID</param>
        /// <param name="endTradeDate">Normally todays date, prefer Y-M-D format</param>
        /// <param name="loginUserEmail">Reports send log back to this emailaddr</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/emailSites")]
        public async Task<IHttpActionResult> EmailSites(int siteId = 0, DateTime? endTradeDate = null, string loginUserEmail = "")
        {
            try
            {
                //Site site = new Site();
                if (endTradeDate == null) endTradeDate = DateTime.Now;

                var listOfSites = new List<Site>();
                var sendLog = new ConcurrentDictionary<int, EmailSendLog>();
                //REMOVE Adding sample data for prices and emails for the moment. 
                //SiteEmail emailsForSite = new SiteEmail();
                //emailsForSite.EmailAddress = "steven.farkas@sainsburys.co.uk";
                //site.Emails.Add(emailsForSite);

                if (siteId != 0)
                {
                    var site = _siteService.GetSitesWithEmailsAndPrices()
                        .FirstOrDefault(x => x.Id == siteId);
                    if (site != null) listOfSites.Add(site);
                }
                else
                {
                    listOfSites = _siteService.GetSitesWithEmailsAndPrices().ToList();
                }
                if (listOfSites.Any())
                {
                    sendLog = await _emailService.SendEmailAsync(listOfSites, endTradeDate.Value, loginUserEmail);
                    // We continue sending on failure.. Log shows which passed or failed
                }

                List<EmailSendLog> logEntries = sendLog.AsParallel().Select(s => s.Value).ToList();
                logEntries = await _emailService.SaveEmailLogToRepositoryAsync(logEntries);
                return Ok(logEntries); // return a List<EmailSendLog> 
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId">0 to send for all sites, otherwise specific siteID</param>
        /// <param name="forDate"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/getEmailSendLog")]
        public async Task<IHttpActionResult> GetEmailSendLog(int siteId = 0, DateTime? forDate = null)
        {
            try
            {
                //Site site = new Site();
                if (forDate == null) forDate = DateTime.Now;

                List<EmailSendLog> sendLog = await _emailService.GetEmailSendLog(siteId, forDate);
                // We continue sending on failure.. Log shows which passed or failed

                return Ok(sendLog); // return a List<EmailSendLog> 
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }


        /// <summary>
        /// Test email body replacement values, usable for UI click to get full email Html with prices
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="endTradeDate"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/ShowEmailBody")]
        public HttpResponseMessage ShowSitesEmailBody(int siteId = 0, DateTime? endTradeDate = null)
        {
            if (endTradeDate == null) endTradeDate = DateTime.Now;

            var listOfSites = new List<Site>();
            var emailBodies = new List<string>();

            if (siteId != 0)
            {
                var site = _siteService.GetSitesWithEmailsAndPrices().FirstOrDefault(x => x.Id == siteId);
                if (site != null) listOfSites.Add(site);
            }
            else
            {
                listOfSites = _siteService.GetSitesWithEmailsAndPrices().ToList();
            }
            if (listOfSites.Any())
            {
                emailBodies.AddRange(listOfSites.Select(site => EmailService.BuildEmailBody(site, endTradeDate.Value)));
            }
            var htmlListOfEmails = String.Join("<hr>", emailBodies);
            var listOfHtmlEmail = new ContentResult()
            {
                ContentType = "text/html",
                Content = htmlListOfEmails
            };

            var response = new HttpResponseMessage();
            response.Content = new StringContent(htmlListOfEmails);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
