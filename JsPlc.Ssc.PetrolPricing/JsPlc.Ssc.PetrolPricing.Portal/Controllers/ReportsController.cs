using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System.Linq;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg = "")
        {
            ViewBag.Message = msg;

            return View();
        }

        [HttpGet]
        public ActionResult CompetitorSites()
        {
            var vm = new CompetitorSiteViewModel();
            Load(vm);
            
            return View(vm);
        }

        [HttpPost]
        public ActionResult CompetitorSites(CompetitorSiteViewModel item)
        {
            if (ModelState.IsValid)
            {
            }
            
            Load(item);
            return View(item);
        }

        private void Load(CompetitorSiteViewModel item)
        {
            item.Sites = _serviceFacade.GetSites().OrderBy(x => x.SiteName).ToList();
        }
    }
}