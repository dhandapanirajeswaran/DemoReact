using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    
    public class SitesController : BaseController
    {

        public SitesController() { }

        public SitesController(IPetrolPricingRepository repository) : base(repository) { }

        [HttpGet] // api/sites/?name=some site name
        public IHttpActionResult GetSite([FromUri]int id)
        {
            var site = _db.GetSite(id);

             if(site==null)
                return NotFound();

            return Ok(site);
        }
        
        [HttpGet]
        [Route("sites")] // /sites
        public IHttpActionResult GetSites()
        {
            var sites = _db.GetSites();

            if (sites == null)
                return NotFound();

            return Ok(sites);
        }

    }
}
