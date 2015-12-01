using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class SitesController : BaseController
    {
        public SitesController() { }

        public SitesController(SiteService siteService) : base(null, siteService, null) { }

        [HttpGet] 
        //[Route("api/site/{id}")] // Not needed but works
        public IHttpActionResult Get([FromUri]int id)
        {

            var site = _siteService.GetSite(id);

             if(site==null || site.Id == 0)
                return NotFound();

            return Ok(site);
        }
        
        [HttpGet]
        //[Route("api/sites")]
        public IHttpActionResult Get()
        {
            var sites = _siteService.GetSites();

            if (sites == null)
                return NotFound();

            return Ok(sites);
        }

        [HttpPost] // Create new site
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


        [HttpPut] // Edit new site
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
        /// 
        /// Later we extend it to:
        /// 1. Calc prices for a given date
        /// 2. Updates SitePrice table with calculated Prices, returns a bool - True if any calcs done for that site, else false
        /// 3. Return type will have to change when calculating Prices for a given date.. Multiple sites may have multiple outcomes of price calcs (some success, some fails)
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> CalcPrice(int siteId)
        {
            // returns a SitePrice object, maybe later we call this for multiple fuels of the site
            // var sitePrices = new List<SitePrice>();

            return Ok(_priceService.CalcPrice(siteId, 1));
        }
    }
}
