using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportViewModel
    {
        public string FuelTypeName { get; set; }

        public List<PricePointReportRowViewModel> PricePointReportRows { get; set; }

        public PricePointReportViewModel()
        {
            PricePointReportRows = new List<PricePointReportRowViewModel>();
        }
    }
}
