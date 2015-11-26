using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class SitesController : BaseController
    {
        public SitesController() { }

        public SitesController(SiteService siteService) : base(null, siteService) { }

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
    }
}
