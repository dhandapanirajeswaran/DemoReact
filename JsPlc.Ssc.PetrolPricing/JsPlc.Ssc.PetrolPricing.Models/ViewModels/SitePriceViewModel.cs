using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    // REUSE same model for competitor information
    public class SitePriceViewModel
    {
        public int SiteId { get; set; } // Holds competitorId, when used for Competitor VM
        public int JsSiteId { get; set; } // Always a JsSiteId, not populated when this VM is used for JsSites ONLY
        public int? StoreNo { get; set; } // NA when CompetitorVM
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }

        public int? CatNo { get; set; } // NA when CompetitorVM
        public int? PfsNo { get; set; } // NA when CompetitorVM

        // Only applicable when used as Competitor VM
        public float? DriveTime { get; set; } // NA when JsSites VM
        public float? Distance { get; set; } // NA when JsSites VM

        public List<FuelPriceViewModel> FuelPrices { get; set; } // list item contains price for each fuel
    }

    // each fuel's price
    public class FuelPriceViewModel
    {
        public int FuelTypeId { get; set; }

        // Tomorrow's Prices 
        public int? AutoPrice { get; set; } // from db
        public int? OverridePrice { get; set; } // from db & user input (only used for JS Sites)
        // Tomorrow's prices would be blank for the Competitor model

        // Todays's Prices
        public int? TodayPrice { get; set; } // from db

        // Yesterday's Prices
        public int? YestPrice { get; set; } // from db
    }
}
