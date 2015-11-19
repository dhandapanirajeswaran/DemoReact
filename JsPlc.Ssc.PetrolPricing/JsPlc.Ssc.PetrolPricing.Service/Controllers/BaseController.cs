using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly IPetrolPricingRepository _db;

        public BaseController()
        {

            _db=new PetrolPricingRepository(new RepositoryContext());
        }

        public BaseController(IPetrolPricingRepository repository)
        {
            _db = repository;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
