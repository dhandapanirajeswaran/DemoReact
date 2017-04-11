using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Http.Results;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using Newtonsoft.Json;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
   
    public class PPUserController : ApiController
    {
        IPPUserService _ppUserService;
        IAppSettings _appSettings;

  
        public PPUserController(IPPUserService ppUserService, IAppSettings appSettings)
        {
            _ppUserService = ppUserService;
            _appSettings = appSettings;
        }


        [System.Web.Http.HttpPost]
        [Route("api/PPUsers/Add")]
        public PPUserList AddUser()
        {
            var queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            var email = queryString["email"];
            var firstname = queryString["firstname"];
            var lastname = queryString["lastname"];

            PPUser user = new PPUser
            {
                Email = email,
                FirstName = firstname,
                LastName = lastname
            };
            
            return _ppUserService.AddUser(user); ;
        }

        [System.Web.Http.HttpPost]
        [Route("api/PPUsers/Delete")]
        public PPUserList DeleteUser()
        {
            var queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

            var email = queryString["email"];

            var usersList = _ppUserService.DeleteUser(email);
            return usersList;
        }

        [System.Web.Http.HttpGet]
        [Route("api/PPUsers")]
         public IHttpActionResult Get()
        {
            var usersList = _ppUserService.GetPPUsers();
            return Ok(usersList);
        }

        [System.Web.Http.HttpGet]
        [Route("api/PPUsers/Edit")]
        public IHttpActionResult Edit(int id)
        {
            var model = _ppUserService.GetPPUserDetails(id);
            return Ok(model);
        }

        [HttpGet]
        [Route("api/PPUsers/Permissions")]
        public IHttpActionResult GetPermissions([FromUri] int ppUserId)
        {
            var permissions = _ppUserService.GetPermissions(ppUserId);
            return Ok(permissions);
        }

        [HttpPost]
        [Route("api/PPUsers/Permissions")]
        public IHttpActionResult UpsertPermissions([FromBody] int requestingPPUserId, [FromBody] PPUserPermissions permissions)
        {
            var result = _ppUserService.UpsertPermissions(requestingPPUserId, permissions);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/PPUsers/Access")]
        public IHttpActionResult GetUserAccess([FromUri] string userName)
        {
            var result = _ppUserService.GetUserAccess(userName);
            return Ok(result);
        }
    }
}
