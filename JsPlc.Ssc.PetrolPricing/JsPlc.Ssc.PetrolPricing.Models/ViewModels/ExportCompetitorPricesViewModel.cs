using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class ExportCompetitorPricesViewModel
    {
        public IEnumerable<SitePriceViewModel> SainsburysSitePrices { get; set; } = new List<SitePriceViewModel>();
        public IEnumerable<SitePriceViewModel> CompetitorPrices { get; set; } = new List<SitePriceViewModel>();

        public DriveTimeFuelSettingsViewModel DriveTimeMarkups { get; set; } = new DriveTimeFuelSettingsViewModel();
    }
}
