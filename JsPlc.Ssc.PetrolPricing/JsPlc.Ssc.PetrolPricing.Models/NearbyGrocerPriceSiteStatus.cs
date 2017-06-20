using JsPlc.Ssc.PetrolPricing.Models.Enums;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class NearbyGrocerPriceSiteStatus
    {
        public int SiteId { get; set; }

        public NearbyGrocerStatuses Unleaded { get; set; }
        public NearbyGrocerStatuses Diesel { get; set; }
        public NearbyGrocerStatuses SuperUnleaded { get; set; }
    }
}