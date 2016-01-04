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
        public string SiteId { get; set; } // JSSiteId
        [Required]
        public string FuelTypeId { get; set; } // 1,2,6 values

        //[Range(typeof(float), "0.00", "400.0", ErrorMessage = "Override price should be between 0 and 400")] // prevent user postback of prices over £4
        //[Required(AllowEmptyStrings = true)]
        public string OverridePrice { get; set; } // from user input 
    }
}
