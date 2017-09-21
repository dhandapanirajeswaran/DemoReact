using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteDetailsViewModel
    {
        public SiteViewModel Site { get; set; } = new SiteViewModel();

        public IEnumerable<NearbySiteViewModel> NearbySites { get; set; } = new List<NearbySiteViewModel>();
    }
}
