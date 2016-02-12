using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class NationalAverageReportViewModel
    {
        public List<NationalAverageReportFuelViewModel> Fuels { get; set; }
        
        public NationalAverageReportViewModel()
        {
            Fuels = new List<NationalAverageReportFuelViewModel>();
        }
    }
}
