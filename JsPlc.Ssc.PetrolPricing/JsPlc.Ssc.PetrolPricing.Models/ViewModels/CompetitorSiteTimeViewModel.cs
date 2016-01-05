using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteTimeViewModel
    {
        public int From { get; set; }
        public int To { get; set; }

        public IEnumerable<CompetitorSiteDetailViewModel> Details { get; set; }

        public CompetitorSiteTimeViewModel()
        {
            Details = new List<CompetitorSiteDetailViewModel>();
        }
    }
}
