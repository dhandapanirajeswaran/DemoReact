using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ISystemSettingsService
    {
        SystemSettings GetSystemSettings();
        SystemSettings UpdateSystemSettings(SystemSettings model);

        SitePricingSettings GetSitePricingSettings();

        IEnumerable<DriveTimeMarkup> GetAllDriveTimeMarkups();

        StatusViewModel UpdateDriveTimeMarkups(IEnumerable<DriveTimeMarkup> driveTimeMarkups);
    }
}
