using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Business;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly FileService _fileService;
        protected readonly SiteService _siteService;

        public BaseController()
        {
            _fileService = new FileService();
            _siteService = new SiteService();
        }

        public BaseController(FileService fileService, SiteService siteService)
        {
            _fileService = fileService;
            _siteService = siteService;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
