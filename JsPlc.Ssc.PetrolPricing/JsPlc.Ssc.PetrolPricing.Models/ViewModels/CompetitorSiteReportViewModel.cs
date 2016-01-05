using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteReportViewModel
    {
        public IEnumerable<CompetitorSiteTimeViewModel> Items { get; set; }

        public CompetitorSiteReportViewModel()
        {
            Items = new List<CompetitorSiteTimeViewModel>();
        }
    }
}
