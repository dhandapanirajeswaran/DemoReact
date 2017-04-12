using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class NearbyGrocerPriceSiteStatus
    {
        public int SiteId { get; set; }
        public bool HasNearbyCompetitorDieselPrice { get; set; }
        public bool HasNearbyCompetitorUnleadedPrice { get; set; }
        public bool HasNearbyCompetitorSuperUnleadedPrice { get; set; }
    }
}
