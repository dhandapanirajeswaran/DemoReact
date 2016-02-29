using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorsPriceRangeByCompanyViewModel
    {
        public DateTime Date { get; set; }

        public Dictionary<string, int> Companies { get; set; }

        public string SelectedCompanyName { get; set; }

        public List<int> FuelTypeIds { get; set; }

        public List<CompetitorsPriceRangeByCompanyCompanyViewModel> ReportCompanies { get; set; }

        public List<FuelType> FuelTypes { get; set; }

        public Dictionary<int, int> SainsburysPrices { get; set; }

        public List<string> Brands { get; set; }

        public string SelectedBrandName { get; set; }

        public CompetitorsPriceRangeByCompanyViewModel()
        {
            Date = DateTime.Today;
            SelectedCompanyName = "All";
            SelectedBrandName = "All";
            Companies = new Dictionary<string, int>();
            Companies.Add(SelectedCompanyName, 0);
            Brands = new List<string>();
            Brands.Add(SelectedBrandName);
            ReportCompanies = new List<CompetitorsPriceRangeByCompanyCompanyViewModel>();
            FuelTypeIds = new List<int> { (int)FuelTypeItem.Diesel, (int)FuelTypeItem.Unleaded };
            SainsburysPrices = new Dictionary<int, int>();
        }
    }
}
