using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class LookupController : BaseController
    {
        [HttpGet]
        [Route("api/UploadTypes")]
        public IHttpActionResult UploadTypes()
        {
            return Ok(LookupService.GetUploadTypes().ToArray());
        }

        [HttpGet]
        [Route("api/FuelTypes")]
        public IHttpActionResult FuelTypes()
        {
            return Ok(LookupService.GetFuelTypes().ToArray());
        }

        [HttpGet]
        [Route("api/ProcessStatuses")]
        public IHttpActionResult ProcessStatuses()
        {
            return Ok(LookupService.GetProcessStatuses().ToArray());
        }

    }
}
