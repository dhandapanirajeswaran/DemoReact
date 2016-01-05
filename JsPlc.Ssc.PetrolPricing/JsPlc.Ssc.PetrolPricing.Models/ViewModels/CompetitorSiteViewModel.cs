using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteViewModel
    {
        [DisplayName("Site")]
        public IEnumerable<Site> Sites { get; set; }

        public int SiteId { get; set; }
        
        public CompetitorSiteViewModel()
        {
            Sites = new List<Site>();
        }
    }
}
