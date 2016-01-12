using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportContainerViewModel
    {
        public String For { get; set; }
        public DateTime? ForDate { get; set; }

        public List<PricePointReportViewModel> PricePointReports { get; set; }

        public PricePointReportContainerViewModel()
        {
            PricePointReports = new List<PricePointReportViewModel>();
        }
    }
}
