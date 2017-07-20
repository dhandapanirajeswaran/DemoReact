using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteCompetitorPriceSummaryViewModel
    {
        public List<SiteCompetitorPriceSummaryRowViewModel> PriceSummaries { get; set; } = new List<SiteCompetitorPriceSummaryRowViewModel>();
    }

    public class SiteCompetitorPriceSummaryRowViewModel
    {
        public int SiteId { get; set; } = 0;
        public FuelTypeItem FuelTypeId { get; set; }
        public int CompetitorCount { get; set; } = 0;
        public int CompetitorPriceCount { get; set; } = 0;
        public int CompetitorPricePercent { get; set; } = 0;
        public int GrocerCount { get; set; } = 0;
        public int GrocerPriceCount { get; set; } = 0;
        public int GrocerPricePercent { get; set; } = 0;
    }
}