using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class BrandItemSettingsViewModel
    {
        public string BrandName { get; set; }
        public bool IsGrocer { get; set; }
        public bool IsExcluded { get; set; }
    }
}
