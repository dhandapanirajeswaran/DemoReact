using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;


namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
   
    [Authorize]
    public class AdminController : Controller
    {
        readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;
        public AdminController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        [HttpGet]
        public ActionResult Index(string option = "")
        {
            return RedirectToAction("ReInitDb");
        }

        [HttpGet]
        [Authorize(Users = "Parveen.Kumar@sainsburys.co.uk, admin@sainsburys.co.uk")]
        public async Task<ActionResult> ReInitDb(string option = "")
        {
            // Display list of existing files along with their status
            if (option == "") return View(model: "");

            using (var svc = new ServiceFacade(_logger))
            {
                string msg = await svc.ReInitDb(option);
                ViewBag.Message = msg;
                return View(model: msg);
            }
        }
    }
}