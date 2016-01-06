using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteReportViewModel
    {
        public IEnumerable<CompetitorBrandTimeViewModel> BrandTimes { get; set; }

        public string SiteName { get; set; }
        
        public CompetitorSiteReportViewModel()
        {
            BrandTimes = new List<CompetitorBrandTimeViewModel>();
        }
    }
}
