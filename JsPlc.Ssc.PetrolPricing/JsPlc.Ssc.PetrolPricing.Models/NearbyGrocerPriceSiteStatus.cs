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
        public bool HasNearbyGrocerPrice { get; set; }
    }
}
