using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class GrocerBrandName
    {
        public int GrocerId { get; set; }
        public string BrandName { get; set; }
        public int BrandId { get; set; }
        public bool IsExcludedBrand { get; set; }
    }
}
