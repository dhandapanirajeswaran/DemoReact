using System.Linq;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
