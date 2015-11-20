using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    
    public class FileController : BaseController
    {

        public FileController() { }

        public FileController(IPetrolPricingRepository repository) : base(repository) { }

    }
}
