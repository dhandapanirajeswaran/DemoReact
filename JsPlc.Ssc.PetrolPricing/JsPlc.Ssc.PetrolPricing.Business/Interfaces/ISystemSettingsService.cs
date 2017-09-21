using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ISystemSettingsService
    {
        SystemSettings GetSystemSettings();

        SystemSettings UpdateSystemSettings(SystemSettings model);

        SitePricingSettings GetSitePricingSettings();

        IEnumerable<DriveTimeMarkup> GetAllDriveTimeMarkups();

        StatusViewModel UpdateDriveTimeMarkups(IEnumerable<DriveTimeMarkup> driveTimeMarkups);

        BrandsCollectionSettingsViewModel GetBrandCollectionSettings();

        StatusViewModel UpdateBrandCollectionSettings(BrandsSettingsUpdateViewModel brandsCollectionSettings);

        BrandsCollectionSummaryViewModel GetBrandCollectionSummary();

        IEnumerable<ScheduleItemViewModel> GetWinServiceScheduledItems();

        IEnumerable<ScheduleEventLogViewModel> GetWinServiceEventLog();

        ScheduleItemViewModel GetWinServiceScheduleItem(int winServiceScheduleId);

        ScheduleItemViewModel UpsertWinServiceSchedule(ScheduleItemViewModel model);

        StatusViewModel RunWinServiceSchedule();

        StatusViewModel ClearWinServiceEventLog();

        string ExportSettings();
        StatusViewModel ImportSettings(ImportSettingsPageViewModel model);
    }
}