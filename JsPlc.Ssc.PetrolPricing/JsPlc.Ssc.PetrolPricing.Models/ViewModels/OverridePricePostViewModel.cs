using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    // Used to Postback data from Pricing page
    public class OverridePricePostViewModel
    {
        [Required]
        public int SiteId { get; set; } // JSSiteId
        
        [Required]
        public int FuelTypeId { get; set; } // 1,2,6 values

        public float OverridePrice { get; set; } // from user input 
    }
}
