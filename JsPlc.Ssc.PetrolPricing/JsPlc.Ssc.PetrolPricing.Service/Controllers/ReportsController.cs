using JsPlc.Ssc.PetrolPricing.Business;
using System;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

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

        /// <summary>
        /// National average 2 report
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetNationalAverage2/{when}")]
        public IHttpActionResult GetNationalAverage2([FromUri]DateTime when)
        {
            var rs = new ReportService();
            var result = rs.GetReportNationalAverage2(when);
            return Ok(result);
        }

        /// <summary>
        /// Competitors Price Range By Company report
        /// </summary>
        /// <param name="when"></param>
        /// <param name="companyName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetCompetitorsPriceRangeByCompany/{when}/{companyName}/{brandName}")]
        public IHttpActionResult GetCompetitorsPriceRangeByCompany([FromUri]DateTime when, [FromUri]string companyName, [FromUri]string brandName)
        {
            var rs = new ReportService();
            var result = rs.GetReportCompetitorsPriceRangeByCompany(when, companyName, brandName);
            return Ok(result);
        }

        /// <summary>
        /// Compliance Report
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetComplianceReport/{when}")]
        public IHttpActionResult GetComplianceReport([FromUri]DateTime when)
        {
            var rs = new ReportService();
            var result = rs.GetReportCompliance(when);
            return Ok(result);
        }

        /// <summary>
        /// PriceMovement Report
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetPriceMovement/{from}/{to}/{fuelTypeId}/{brandName}")]
        public IHttpActionResult GetPriceMovement([FromUri]string brandName, [FromUri]DateTime from, [FromUri]DateTime to, [FromUri]int fuelTypeId)
        {
            var rs = new ReportService();
            var result = rs.GetReportPriceMovement(brandName, from, to, fuelTypeId);
            return Ok(result);
        }
    }
}
