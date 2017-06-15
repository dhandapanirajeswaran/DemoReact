using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings
{
    public class DriveTimeMarkupFuelViewModel
    {
        public int FuelTypeId { get; set; }

        public string PanelId { get; set; }

        public IEnumerable<DriveTimeMarkupViewModel> DriveTimeMarkups { get; set; }

        public DriveTimeMarkupFuelViewModel()
        {
            this.DriveTimeMarkups = new List<DriveTimeMarkupViewModel>();
        }
    }
}
