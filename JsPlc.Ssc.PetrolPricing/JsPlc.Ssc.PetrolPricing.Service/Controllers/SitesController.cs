﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;

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

             if(site==null || site.Id == 0)
                return NotFound();

            return Ok(site);
        }
        
        [System.Web.Http.HttpGet]
        //[Route("api/sites")]
        public IHttpActionResult Get()
        {
            var sites = _siteService.GetSites();

            if (sites == null)
                return NotFound();

            return Ok(sites);
        }

        [System.Web.Http.HttpPost] // Create new site
        public async Task<IHttpActionResult> Post(Site site)
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
                    var su = ss.NewSite(site);
                    return Ok(su);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }


        [System.Web.Http.HttpPut] // Edit new site
        public async Task<IHttpActionResult> Update(Site site)
        {
            if (site == null)
            {
                return BadRequest("Invalid passed data: site");
            }

            try
            {
                using (var ss = _siteService)
                {
                    ss.UpdateSite(site);
                    return Ok(site);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        /// <summary>
        /// Calculates Prices for a given site (as per Catalist upload of today) as a test, 
        /// Later we extend it to:
        /// 1. Calc prices for a given date
        /// 2. Updates SitePrice table with calculated Prices, returns a bool - True if any calcs done for that site, else false
        /// 3. Return type will have to change when calculating Prices for a given date.. Multiple sites may have multiple outcomes of price calcs (some success, some fails)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fuelId"></param>
        /// <returns>SitePrice</returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/CalcPrice/")]
        public async Task<IHttpActionResult> CalcPrice([FromUri]int siteId, [FromUri] int fuelId)
        {
            // returns a SitePrice object, maybe later we call this for multiple fuels of the site

            // Test for 30 Nov prices as we have a dummy set of these setup
            // We dont have any 1st Dec prices
            SitePrice price = null;

            price = _priceService.CalcPrice(siteId, fuelId, DateTime.Parse("2015-11-30")); // Unleaded
            return Ok(price);
        }

        /// <summary>
        ///  Not used yet, churns out a lot of data
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/SiteDetails/")]
        public IHttpActionResult GetSitesWithPricesAndCompetitors()
        {
            var sites =  _siteService.GetSitesWithPricesAndCompetitors();
            return Ok(sites);
        }

        /// <summary>
        /// Gets a list of SitePriceViewModel for SitePricing tab main data
        /// Test Url: api/SitePrices?forDate=2015-11-30&siteId=1 
        /// or api/SitePrices
        /// </summary>
        /// <param name="forDate">Optional - Date of Calc/Viewing</param>
        /// <param name="siteId">Optional - Specific SiteId or 0 for all Sites</param>
        /// <param name="pageNo">Optional - Viewing PageNo</param>
        /// <param name="pageSize">Optional - PageSize, set large value (e.g. 1000) to get all sites</param>
        /// <returns>List of SitePriceViewModel</returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/SitePrices")]
        public IHttpActionResult GetSitesWithPrices([FromUri] DateTime? forDate=null,
            [FromUri]int siteId = 0, [FromUri]int pageNo = 1, [FromUri]int pageSize = Constants.PricePageSize)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            var siteWithPrices = _siteService.GetSitesWithPrices(forDate.Value, siteId, pageNo, pageSize);
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
                string mailFrom = ConfigurationManager.AppSettings["emailFrom"]; // "akiaip5@gmail.com";
                const string mailTo = "somesiteEmail@sainsburys.co.uk"; //"akiaip5@gmail.com";
                const string mailSubject = "Hello, Test Email from Gmail SMTP 587";
                const string mailBody = "<h1>Hello, This is a <span syle='color: red'>Test Email from Smtp</span> from C# code</h1>";

                // Send the email. 
                try
                {
                    Debug.WriteLine("TestSendMail: Attempting to send an email through the Amazon SES SMTP interface...");
                    var mailMsg = new MailMessage(mailFrom, mailTo) { 
                        IsBodyHtml = true, 
                        Subject = mailSubject,
                        Body = mailBody,
                        };
                    client.Send(mailMsg);
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
    }
}
