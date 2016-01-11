using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteViewModel
    {
        [DisplayName("Site")]
        public List<Site> Sites { get; set; }

        public int SiteId { get; set; }

        public CompetitorSiteReportViewModel Report { get; set; }
        
        public CompetitorSiteViewModel()
        {
            Sites = new List<Site>();
            Report = new CompetitorSiteReportViewModel();
        }
    }
}
