using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class ImportSettingsPageViewModel
    {
        public StatusViewModel Status { get; set; } = new StatusViewModel();

        public string SettingsXml { get; set; } = "";
        public bool ImportCommonSettings { get; set; } = false;
        public bool ImportDriveTimeMarkup { get; set; } = false;
        public bool ImportGrocers { get; set; } = false;
        public bool ImportExcludedBrands { get; set; } = false;
        public bool ImportPriceFreezeEvents { get; set; } = false;
    }
}
