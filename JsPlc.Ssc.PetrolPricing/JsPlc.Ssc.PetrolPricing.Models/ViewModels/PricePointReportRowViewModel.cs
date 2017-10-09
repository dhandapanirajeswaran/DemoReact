using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportRowViewModel
    {
        public string Brand { get; set; }

        public ReportRowType ReportRowType { get; set; }

        public List<PricePointPriceViewModel> PricePointPrices { get; set; }

        public PricePointReportRowViewModel()
        {
            PricePointPrices = new List<PricePointPriceViewModel>();
        }
    }
}
