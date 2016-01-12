using JsPlc.Ssc.PetrolPricing.Business;
using System;
using System.Web.Http;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class ReportsController : BaseController
    {
        /// <summary>
        /// Competitor Sites report
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetCompetitorSites/{siteId}")]
        public IHttpActionResult GetCompetitorSites([FromUri]int siteId = 0)
        {
            var rs = new ReportService();
            var result = rs.GetReportCompetitorSites(siteId);
            return Ok(result);
        }

        /// <summary>
        /// PricePoints Report
        /// </summary>
        /// <param name="when"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetPricePoints/{when}/{fuelTypeId}")]
        public IHttpActionResult GetPricePoints([FromUri]DateTime when, [FromUri]int fuelTypeId)
        {
            var rs = new ReportService();
            var result = rs.GetReportPricePoints(when, fuelTypeId);
            return Ok(result);
        }

        /// <summary>
        /// National average report
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetNationalAverage/{when}")]
        public IHttpActionResult GetNationalAverage([FromUri]DateTime when)
        {
            var rs = new ReportService();
            var result = rs.GetReportNationalAverage(when);
            return Ok(result);
        }
    }
}
