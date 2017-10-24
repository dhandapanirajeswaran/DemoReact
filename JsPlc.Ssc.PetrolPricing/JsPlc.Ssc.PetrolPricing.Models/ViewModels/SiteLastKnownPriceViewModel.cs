using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteLastKnownPriceViewModel
    {
        public int SiteId { get; set; }
        public int FuelTypeId { get; set; }
        public int Price { get; set; }
        public DateTime DateOfCalc { get; set; }
        public int SitePriceId { get; set; }
    }
}
