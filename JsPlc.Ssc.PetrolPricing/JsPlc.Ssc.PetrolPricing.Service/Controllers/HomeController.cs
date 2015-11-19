using System.Linq;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            using (var repo = new RepositoryContext())
            {
                var x = repo.Sites.ToList();
            }

            return View();
        }
    }
}
