using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Business;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly FileService _fileService;
        protected readonly SiteService _siteService;
        protected readonly PriceService _priceService;
        protected readonly EmailService _emailService;

        public BaseController()
        {
            _fileService = new FileService();
            _siteService = new SiteService();
            _priceService = new PriceService();
            _emailService = new EmailService();
        }

        public BaseController(FileService fileService, SiteService siteService, 
            PriceService priceService, EmailService emailService=null)
        {
            _fileService = fileService;
            _siteService = siteService;
            _priceService = priceService;
            _emailService = new EmailService(); // TODO change as per others init-ed
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
