using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorPriceViewModel
    {
        public int? CatNo { get; set; }

        public int? CompetitorSiteId { get; set; }
        public int? JsSiteId { get; set; }

        public string Brand { get; set; } 
        public string StoreName { get; set; } // Marker

        public string Address { get; set; } 
        public string Town { get; set; }
        
        public float DriveTime { get; set; } // 2.54 etc 
        public int Rank { get; set; } 
        
        public List<FuelPriceViewModel> FuelPrices { get; set; } // list item contains price for each fuel
    }

}
