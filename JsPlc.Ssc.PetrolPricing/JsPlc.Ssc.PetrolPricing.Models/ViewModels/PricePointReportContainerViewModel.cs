﻿using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PricePointReportContainerViewModel
    {
        public DateTime? For { get; set; }

        public List<PricePointReportViewModel> PricePointReports { get; set; }

        public PricePointReportContainerViewModel()
        {
            PricePointReports = new List<PricePointReportViewModel>();
        }
    }
}