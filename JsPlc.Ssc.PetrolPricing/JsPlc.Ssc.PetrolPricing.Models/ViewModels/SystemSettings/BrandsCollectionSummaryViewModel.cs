using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class BrandsCollectionSummaryViewModel
    {
        public long BrandsCount { get; set; }
        public long GrocerCount { get; set; }
        public long ExcludedCount { get; set; }
    }
}
