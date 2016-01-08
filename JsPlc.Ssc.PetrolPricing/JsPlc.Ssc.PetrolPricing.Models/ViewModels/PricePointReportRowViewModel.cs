using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportRowViewModel
    {
        public decimal Price { get; set; }

        public List<PricePointBrandViewModel> PricePointBrands { get; set; }

        public PricePointReportRowViewModel()
        {
            PricePointBrands = new List<PricePointBrandViewModel>();
        }
    }
}
