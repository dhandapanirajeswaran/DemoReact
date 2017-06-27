using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class BrandsCollectionSettingsViewModel
    {
        public IEnumerable<BrandItemSettingsViewModel> BrandSettings { get; set; }

        public BrandsCollectionSettingsViewModel()
        {
            this.BrandSettings = new List<BrandItemSettingsViewModel>();
        }
    }
}
