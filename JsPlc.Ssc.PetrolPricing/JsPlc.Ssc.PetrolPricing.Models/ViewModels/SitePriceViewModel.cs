using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SitePriceViewModel
    {
        public int SiteId { get; set; }
        public int? StoreNo { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }

        public int? CatNo { get; set; }
        public int? PfsNo { get; set; }

        public List<FuelPriceViewModel> FuelPrices { get; set; } // list item contains price for each fuel
    }

    // each fuel's price
    public class FuelPriceViewModel
    {
        public int FuelTypeId { get; set; }

        // Tomorrow's Prices
        public int? AutoPrice { get; set; } // from db
        public int? OverridePrice { get; set; } // from db & user input (only used for JS Sites)

        // Todays's Prices
        public int? TodayPrice { get; set; } // from db
    }

}
