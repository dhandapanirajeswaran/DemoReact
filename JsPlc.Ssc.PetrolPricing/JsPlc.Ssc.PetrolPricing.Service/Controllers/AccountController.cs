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
        private readonly IAccountService _accountService;
        private readonly IPPUserService _userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public AccountController(IAccountService service, IPPUserService userService)
        {
            _accountService = service;
            _userService = userService;
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
            _accountService.RegisterUser(email);

            return Ok();
        }

        [HttpPost]
        [Route("api/signin")]
        public IHttpActionResult SignIn([FromUri] string email)
        {
            _userService.SignIn(email);
            return Ok();
        }

    }
}
