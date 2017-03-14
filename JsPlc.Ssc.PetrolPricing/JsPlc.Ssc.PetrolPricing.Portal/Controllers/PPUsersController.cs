using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
   
    [System.Web.Mvc.Authorize]
    public class PPUsersController : Controller
    {
        private readonly ServiceFacade _serviceFacade ;
        private readonly ILogger _logger;

        public PPUsersController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        public ActionResult Index()
        {
            try
            {
                var model = _serviceFacade.GetPPUsers();
                // Filtering based on search value     
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return View();
            }
        }


        public ActionResult Adduser(String firstname, String lastName, String email)
        {
            PPUser user = new PPUser { Email = email, LastName = lastName, FirstName = firstname };

            var result = _serviceFacade.AddPPUser(user);

            return View("Index", result);
        }

        public ActionResult DeleteUser(string email)
        {
            var result = _serviceFacade.DeletePPUser(email);
            return View("Index", result);
        }
    }
}

