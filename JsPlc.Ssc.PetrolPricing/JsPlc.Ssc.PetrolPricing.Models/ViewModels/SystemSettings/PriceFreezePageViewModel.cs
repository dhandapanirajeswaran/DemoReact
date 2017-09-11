using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class PriceFreezePageViewModel
    {
        public StatusViewModel Status { get; set; } = new StatusViewModel();
        public IEnumerable<PriceFreezeEventViewModel> PriceFreezeEvents { get; set; } = new List<PriceFreezeEventViewModel>();
    }
}
