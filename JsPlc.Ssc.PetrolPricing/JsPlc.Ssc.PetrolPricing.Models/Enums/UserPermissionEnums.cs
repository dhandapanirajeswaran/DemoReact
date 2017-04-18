using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    [Flags]
    public enum FileUploadsUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Upload = 0x02,

        // Defaults for new user
        NewUserDefaults = FileUploadsUserPermissions.View | FileUploadsUserPermissions.Upload
    }

    [Flags]
    public enum SitesPricingUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Export = 0x02,
        Update = 0x04,

        // Defaults for new user
        NewUserDefaults =  SitesPricingUserPermissions.View | SitesPricingUserPermissions.Export | SitesPricingUserPermissions.Update
    }

    [Flags]
    public enum SitesMaintenanceUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Add = 0x02,
        Edit = 0x04,

        // Defaults for new user
        NewUserDefaults = SitesMaintenanceUserPermissions.View | SitesMaintenanceUserPermissions.Add | SitesMaintenanceUserPermissions.Edit
    }

    [Flags]
    public enum ReportsUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Export = 0x02,

        // Defaults for new user
        NewUserDefaults =  ReportsUserPermissions.View | ReportsUserPermissions.Export
    }

    [Flags]
    public enum UsersManagementUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Add = 0x02,
        Edit = 0x04,
        Delete = 0x08,

        // Defaults for new user
        NewUserDefaults = UsersManagementUserPermissions.View | UsersManagementUserPermissions.Add | UsersManagementUserPermissions.Edit
    }

    [Flags]
    public enum DiagnosticsUserPermissions
    {
        None = 0, // Access Denied
        View = 0x01,
        Edit = 0x02,

        // Defaults for new user
        NewUserDefaults = DiagnosticsUserPermissions.None
    }
}
