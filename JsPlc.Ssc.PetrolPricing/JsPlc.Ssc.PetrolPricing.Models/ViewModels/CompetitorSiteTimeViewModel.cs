using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorSiteTimeViewModel
    {
        public int Count0To5 { get; set; }
        public int Count5To10 { get; set; }
        public int Count10To15 { get; set; }
        public int Count15To20 { get; set; }
        public int Count20To25 { get; set; }

        public string SiteName { get; set; }
    }
}
