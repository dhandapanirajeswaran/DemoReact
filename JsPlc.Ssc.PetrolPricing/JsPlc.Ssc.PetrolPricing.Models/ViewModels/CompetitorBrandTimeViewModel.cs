using System.Collections.Generic;
using System.ComponentModel;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorBrandTimeViewModel
    {
        public int Count0To5 { get; set; }
        public int Count5To10 { get; set; }
        public int Count10To15 { get; set; }
        public int Count15To20 { get; set; }
        public int Count20To25 { get; set; }
        public int Count25To30 { get; set; }
        public int CountMoreThan30 { get; set; }

        public string BrandName { get; set; }
    }
}
