using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Layout
{
    public class TopNavigationViewModel
    {
        public UserAccessViewModel UserAccess { get; set; }

        public TopNavigationViewModel()
        {
            this.UserAccess = new UserAccessViewModel();
        }
    }
}