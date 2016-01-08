using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class NationalAverageReportContainerViewModel
    {
        public DateTime? For { get; set; }

        public NationalAverageReportViewModel NationalAverageReport { get; set; }

        public NationalAverageReportContainerViewModel()
        {
            NationalAverageReport = new NationalAverageReportViewModel();
        }
    }
}
