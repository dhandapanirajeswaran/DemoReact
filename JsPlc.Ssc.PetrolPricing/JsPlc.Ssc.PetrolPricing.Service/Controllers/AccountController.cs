using JsPlc.Ssc.PetrolPricing.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class AccountController : ApiController
    {
        private readonly IAccountService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public AccountController(IAccountService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/user")]
        public IHttpActionResult Register([FromUri]string email)
        {
            _service.RegisterUser(email);

            return Ok();
        }
    }
}
