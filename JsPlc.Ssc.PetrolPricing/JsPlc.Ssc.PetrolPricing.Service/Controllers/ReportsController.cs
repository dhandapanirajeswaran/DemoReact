using JsPlc.Ssc.PetrolPricing.Business;
using System;
using System.Web.Http;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class ReportsController : BaseController
    {
        [HttpGet]
        [Route("api/GetCompetitorSites/{siteId}")]
        public IHttpActionResult GetCompetitorSites([FromUri]int siteId = 0)
        {
            var rs = new ReportService();
            var result = rs.GetCompetitorSites(siteId);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/GetPricePoints/{when}/{fuelTypeId}")]
        public IHttpActionResult GetPricePoints([FromUri]DateTime when, [FromUri]int fuelTypeId)
        {
            var rs = new ReportService();
            var result = rs.GetPricePoints(when, fuelTypeId);
            return Ok(result);
        }
    }
}
