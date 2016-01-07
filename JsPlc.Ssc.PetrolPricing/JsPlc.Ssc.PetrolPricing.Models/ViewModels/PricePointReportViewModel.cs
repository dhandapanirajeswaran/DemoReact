using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportViewModel
    {
        public string FuelTypeName { get; set; }

        public List<PricePointReportItem> PricePointReportItems { get; set; }

        public PricePointReportViewModel()
        {
            PricePointReportItems = new List<PricePointReportItem>();
        }
    }
}
