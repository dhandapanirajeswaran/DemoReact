using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class LookupController : ApiController
    {
        protected readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet]
        [Route("api/UploadTypes")]
        public IHttpActionResult UploadTypes()
        {
            return Ok(_lookupService.GetUploadTypes().ToArray());
        }

        [HttpGet]
        [Route("api/FuelTypes")]
        public IHttpActionResult FuelTypes()
        {
            return Ok(_lookupService.GetFuelTypes().ToArray());
        }

        [HttpGet]
        [Route("api/ProcessStatuses")]
        public IHttpActionResult ProcessStatuses()
        {
            return Ok(_lookupService.GetProcessStatuses().ToArray());
        }

    }
}
