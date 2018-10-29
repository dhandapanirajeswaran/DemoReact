using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class NationalAverageReportContainerViewModel
    {
        public DateTime? ForDate { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public NationalAverageReportViewModel NationalAverageReport { get; set; }

        public NationalAverageReportContainerViewModel()
        {
            NationalAverageReport = new NationalAverageReportViewModel();
        }
    }
}
