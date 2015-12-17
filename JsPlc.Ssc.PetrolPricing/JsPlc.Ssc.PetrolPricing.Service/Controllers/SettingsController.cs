using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
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

        /// <summary>
        /// ReSeed the DB with settings etc, drop n re-create sprocs 
        /// </summary>
        /// <param name="buildOptions">FULLREBUILD, CONFIGKEYSONLY, SPROCSONLY</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ReInitDb")]
        public async Task<IHttpActionResult> ReInitDb(string buildOptions = "")
        {
            try
            {
                switch (buildOptions)
                {
                    case "FULLREBUILD": RepositoryInitializer.SeedRepository(new RepositoryContext());
                        break;
                    case "CONFIGKEYSONLY": RepositoryInitializer.ReInitConfigKeys(new RepositoryContext());
                        break;
                    case "SPROCSONLY": RepositoryInitializer.ReInitSprocs(new RepositoryContext());
                        break;
                    default:
                        throw new Exception("forcing an exception");
                        return await Task.FromResult(BadRequest("Invalid option"));
                }
                return await Task.FromResult(Ok("Success:" + buildOptions));
            }
            catch (Exception ex)
            {
                return Task.FromResult(BadRequest(ex.Message)).Result;
            }
        }
    }
}
