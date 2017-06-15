using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class DriveTimeFuelSettingsViewModel
    {
        public StatusViewModel Status { get; set; }

        public IEnumerable<DriveTimeMarkupViewModel> Unleaded { get; set; }
        public IEnumerable<DriveTimeMarkupViewModel> Diesel { get; set; }
        public IEnumerable<DriveTimeMarkupViewModel> SuperUnleaded { get; set; }

        public DriveTimeFuelSettingsViewModel()
        {
            this.Status = new StatusViewModel();
            this.Unleaded = new List<DriveTimeMarkupViewModel>();
            this.Diesel = new List<DriveTimeMarkupViewModel>();
            this.SuperUnleaded = new List<DriveTimeMarkupViewModel>();
        }
    }
}