using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorsPriceRangeByCompanyCompanyViewModel
    {
        public string CompanyName { get; set; }

        public List<CompetitorsPriceRangeByCompanyBrandViewModel> Brands { get; set; }

        public CompetitorsPriceRangeByCompanyCompanyViewModel()
        {
            Brands = new List<CompetitorsPriceRangeByCompanyBrandViewModel>();
        }
    }
}
