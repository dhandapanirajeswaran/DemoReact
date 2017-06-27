using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private IPetrolPricingRepository _repository;

        public SystemSettingsService(IPetrolPricingRepository repository)
        {
            _repository = repository;
        }

        public SystemSettings GetSystemSettings()
        {
            return _repository.GetSystemSettings();
        }

        public SystemSettings UpdateSystemSettings(SystemSettings settings)
        {
            _repository.UpdateSystemSettings(settings);
            return _repository.GetSystemSettings();
        }

        public SitePricingSettings GetSitePricingSettings()
        {
            return _repository.GetSitePricingSettings();
        }

        public IEnumerable<DriveTimeMarkup> GetAllDriveTimeMarkups()
        {
            return _repository.GetAllDriveTimeMarkups();
        }

        public StatusViewModel UpdateDriveTimeMarkups(IEnumerable<DriveTimeMarkup> driveTimeMarkups)
        {
            return _repository.UpdateDriveTimeMarkup(driveTimeMarkups);
        }

        public BrandsCollectionSettingsViewModel GetBrandCollectionSettings()
        {
            return _repository.GetBrandCollectionSettings();
        }
        public StatusViewModel UpdateBrandCollectionSettings(BrandsSettingsUpdateViewModel brandsCollectionSettings)
        {
            var result = _repository.UpdateBrandCollectionSettings(brandsCollectionSettings);
            if (result)
                return new StatusViewModel()
                {
                    SuccessMessage = "Updated Brands Settings",
                    ErrorMessage = ""
                };
            else
                return new StatusViewModel()
                {
                    SuccessMessage = "",
                    ErrorMessage = "Update to save Brands Settings"
                };
        }
        public BrandsCollectionSummaryViewModel GetBrandCollectionSummary()
        {
            return _repository.GetBrandCollectionSummary();
        }
    }
}
