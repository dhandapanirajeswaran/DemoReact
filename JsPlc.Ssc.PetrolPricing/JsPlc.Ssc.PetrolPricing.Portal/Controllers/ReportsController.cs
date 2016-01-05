using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Portal.Models;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg = "")
        {
            // Display list of existing sites along with their status
            ViewBag.Message = msg;

            //var model = _serviceFacade.GetSites();
            return View();
        }

        [HttpGet]
        public ActionResult CompetitorSites()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CompetitorSites(CompetitorSitesViewModel item)
        {
            return View();
        }
    }
}