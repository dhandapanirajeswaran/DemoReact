using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorsPriceRangeByCompanyBrandViewModel
    {
        public List<CompetitorsPriceRangeByCompanyBrandFuelViewModel> Fuels { get; set; }

        public string BrandName { get; set; }

        public CompetitorsPriceRangeByCompanyBrandViewModel()
        {
            Fuels = new List<CompetitorsPriceRangeByCompanyBrandFuelViewModel>();
        }
    }
}
