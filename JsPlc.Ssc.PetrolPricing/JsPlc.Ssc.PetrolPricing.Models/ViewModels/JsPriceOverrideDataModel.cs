using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class JsPriceOverrideDataModel
    {
        public int CatNo { get; set; }
        public string SiteName { get; set; }
        public int? UnleadedIncrease { get; set; }
        public int? UnleadedAbsolute { get; set; }
        public int? DieselIncrease { get; set; }
        public int? DieselAbsolute { get; set; }
        public int? SuperUnleadedIncrease { get; set; }
        public int? SuperUnleadedAbsolute { get; set; }
    }
}
