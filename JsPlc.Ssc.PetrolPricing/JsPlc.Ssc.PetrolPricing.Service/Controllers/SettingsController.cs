using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class SettingsController : BaseController
    {
        [Route("api/settings/{key}")]
        public async Task<IHttpActionResult> Get(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                return BadRequest("Invalid passed data: setting key");
            }
            try
            {
                    var val = SettingsService.GetSetting(key);
                    return Ok(val);
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }
    }
}
