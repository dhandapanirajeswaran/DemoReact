using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class NationalAverageReportFuelViewModel
    {
        public string FuelName { get; set; }
        public List<NationalAverageReportBrandViewModel> Brands { get; set; }
        public int SainsburysPrice { get; set; }

        public NationalAverageReportFuelViewModel()
        {
            Brands = new List<NationalAverageReportBrandViewModel>();
        }

    }
}
