using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions
{
    public class UserAccessViewModel
    {
        public bool IsUserAuthenticated { get; set; }

        public int PPUserId { get; set; }
        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        public UserFileUploadsAccess UserFileUploadsAccess = new UserFileUploadsAccess();
        public UserSitePricingAccess UserSitePricingAccess = new UserSitePricingAccess();
        public UserSitesMaintenanceAccess UserSitesMaintenanceAccess = new UserSitesMaintenanceAccess();
        public UserReportsAccess UserReportsAccess = new UserReportsAccess();
        public UserUserManagementAccess UserUserManagementAccess = new UserUserManagementAccess();
        public UserDiagnosticsAccess UserDiagnosticsAccess = new UserDiagnosticsAccess();

        public UserAccessViewModel()
        {
            this.IsUserAuthenticated = false;
            this.PPUserId = 0;
            this.UserName = "";
            this.IsAdmin = false;
        }
    }

    public class UserFileUploadsAccess
    {
        public bool CanView { get; set; }
        public bool CanUpload { get; set; }

        public UserFileUploadsAccess()
        {
            this.CanView = false;
            this.CanUpload = false;
        }
    }
    public class UserSitePricingAccess
    {
        public bool CanView { get; set; }
        public bool CanExport { get; set; }
        public bool CanUpdate { get; set; }

        public UserSitePricingAccess()
        {
            this.CanView = false;
            this.CanExport = false;
            this.CanUpdate = false;
        }
    }
    public class UserSitesMaintenanceAccess
    {
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }

        public UserSitesMaintenanceAccess()
        {
            this.CanView = false;
            this.CanAdd = false;
            this.CanEdit = false;
        }
    }
    public class UserReportsAccess
    {
        public bool CanView { get; set; }
        public bool CanExport { get; set; }

        public UserReportsAccess()
        {
            this.CanView = false;
            this.CanExport = false;
        }
    }
    public class UserUserManagementAccess
    {
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        public UserUserManagementAccess()
        {
            this.CanView = false;
            this.CanAdd = false;
            this.CanEdit = false;
            this.CanDelete = false;
        }
    }
    public class UserDiagnosticsAccess
    {
        public bool CanView { get; set; }

        public UserDiagnosticsAccess()
        {
            this.CanView = false;
        }
    }
}
