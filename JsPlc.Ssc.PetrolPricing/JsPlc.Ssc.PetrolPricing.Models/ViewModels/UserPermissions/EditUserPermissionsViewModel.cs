using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions
{
    public class EditUserPermissionsViewModel
    {
        public int PPUserId { get; set; }
        public bool IsAdmin { get; set; }
        public UserPermissionsOptionListViewModel UploadPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel SitesPricingPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel SitesPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel ReportsPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel UsersPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel DiagnosticsPermissionsOptionList { get; set; }
        public UserPermissionsOptionListViewModel SystemSettingsPermissionsOptionList { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
