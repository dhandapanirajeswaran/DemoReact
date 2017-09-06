using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    // REUSE JSSites VM for competitor VM (Pricing page)
    public class SitePriceViewModel
    {
        public int SiteId { get; set; } // Holds competitorId, when used for Competitor VM
        public int JsSiteId { get; set; } // Always a JsSiteId, not populated when this VM is used for JsSites ONLY
        public int? StoreNo { get; set; } // NA when CompetitorVM
        public string Brand { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }

        public int? CatNo { get; set; } // NA when CompetitorVM
        public int? PfsNo { get; set; } // NA when CompetitorVM

        // Only applicable when used as Competitor VM
        public float? DriveTime { get; set; } // NA when JsSites VM
        public float? Distance { get; set; } // NA when JsSites VM

        public bool hasCompetitors { get; set; }
        public List<SitePriceViewModel> competitors { get; set; }

        public List<FuelPriceViewModel> FuelPrices { get; set; } // list item contains price for each fuel

        public bool IsTrailPrice { get; set; } // if true, then trial price has been selected 

        public PriceMatchType PriceMatchType { get; set; }

        public bool HasEmails { get; set; }

        public List<String> Emails { get; set; }
        public bool hasNotes
        {
            get { return String.IsNullOrWhiteSpace(Notes) == false; }
        }
        public string Notes { get; set; }


        public bool HasNearbyUnleadedGrocers { get; set; }
        public bool HasNearbyUnleadedGrocersPriceData { get; set; }

        public bool HasNearbyDieselGrocers { get; set; }
        public bool HasNearbyDieselGrocersPriceData { get; set; }

        public bool HasNearbySuperUnleadedGrocers { get; set; }
        public bool HasNearbySuperUnleadedGrocersPriceData { get; set; }

        public SiteCompetitorPriceSummaryViewModel SiteCompetitorsInfo { get; set; } = new SiteCompetitorPriceSummaryViewModel();

        public bool IsGrocer { get; set; }

        // Competitor site information (Competitor popup modal)
        public bool IsExcludedBrand { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsActive { get; set; }
    }

    // each fuel's price
    public class FuelPriceViewModel
    {
        public int SiteId { get; set; }

        public int FuelTypeId { get; set; }

        // Tomorrow's Prices (only available when buidling JSSites VM, not for competitors VM)
        public int? AutoPrice { get; set; } // from db
        public int? OverridePrice { get; set; } // from db & user input (only used for JS Sites)
        // Tomorrow's prices would be blank for the Competitor model

        // Todays's Prices
        public int? TodayPrice { get; set; } // from db
        public DateTime? TodayDate { get; set; } // competitor VM

        // Yesterday's Prices
        public int? YestPrice { get; set; } // from db
        public DateTime? YestDate { get; set; } // competitor VM

        public bool IsSainsburysSite { get; set; } // competitor VM

        // Markup of the selected Competitor
        public int? Markup { get; set; }

        // Selected Competitor
        public string CompetitorName { get; set; }

        public bool IsTrailPrice { get; set; }

        public int? Difference { get; set; }

        public double CompetitorPriceOffset { get; set; }

        public string PriceSource { get; set; }
        public string PriceSourceDateTime { get; set; }
        public int CompetitorSiteId { get; set; } // 0 = none
        public double Distance { get; set; }
        public double DriveTime { get; set; }
        public int DriveTimePence { get; set; }
        public bool IsBasedOnCompetitor { get; set; }
        public bool HasNearbyCompetitorPrice { get; set; }
        public bool HasNearbyCompetitorWithOutPrice { get; set; }

        public int MatchCompetitorMarkup { get; set; }

        public PriceReasonFlags TodayPriceReasonFlags { get; set; }
        public PriceReasonFlags TomorrowPriceReasonFlags { get; set; }

        public int DriveTimeMarkup { get; set; }
        public int CompetitorCount { get; set; }
        public int CompetitorPriceCount { get; set; }
        public int GrocerCount { get; set; }
        public int GrocerPriceCount { get; set; }
        public int NearbyGrocerCount { get; set; }
        public int NearbyGrocerPriceCount { get; set; }
    }
}
