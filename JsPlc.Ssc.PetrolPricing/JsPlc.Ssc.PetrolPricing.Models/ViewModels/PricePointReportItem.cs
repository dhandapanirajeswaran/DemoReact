using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportItem
    {
        public decimal Price { get; set; }

        public List<PricePointBrandCount> PricePointBrands { get; set; }

        public PricePointReportItem()
        {
            PricePointBrands = new List<PricePointBrandCount>();
        }
    }
}
