using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Facade
{
    public static class UserPermissionsOptionLists
    {
        public static IEnumerable<UserPermissionsOptionViewModel> UserUploadsPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(FileUploadsUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(FileUploadsUserPermissions.View, "View Only" ),
                new UserPermissionsOptionViewModel(FileUploadsUserPermissions.View | FileUploadsUserPermissions.Upload, "View and Upload" )
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserSitesPricingPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(SitesPricingUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(SitesPricingUserPermissions.View, "View Only"),
                new UserPermissionsOptionViewModel(SitesPricingUserPermissions.View | SitesPricingUserPermissions.Export, "View and Export"),
                new UserPermissionsOptionViewModel(SitesPricingUserPermissions.View | SitesPricingUserPermissions.Update, "View and Update"),
                new UserPermissionsOptionViewModel(SitesPricingUserPermissions.View | SitesPricingUserPermissions.Update | SitesPricingUserPermissions.Export, "View, Update and Export")
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserSitesPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(SitesMaintenanceUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(SitesMaintenanceUserPermissions.View, "View Only"),
                new UserPermissionsOptionViewModel(SitesMaintenanceUserPermissions.View | SitesMaintenanceUserPermissions.Add | SitesMaintenanceUserPermissions.Edit, "View, Add and Edit")
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserReportsPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(ReportsUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(ReportsUserPermissions.View, "View Only"),
                new UserPermissionsOptionViewModel(ReportsUserPermissions.View | ReportsUserPermissions.Export, "View and Export")
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserUsersPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(UsersManagementUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(UsersManagementUserPermissions.View, "View Only"),
                new UserPermissionsOptionViewModel(UsersManagementUserPermissions.View | UsersManagementUserPermissions.Add | UsersManagementUserPermissions.Edit, "View, Add and Edit"),
                new UserPermissionsOptionViewModel(UsersManagementUserPermissions.View | UsersManagementUserPermissions.Add | UsersManagementUserPermissions.Edit | UsersManagementUserPermissions.Delete, "View, Add, Edit and Delete")
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserDiagnosticsPermissionsOptions = new List<UserPermissionsOptionViewModel>()
            {
                new UserPermissionsOptionViewModel(DiagnosticsUserPermissions.None, "Access Denied"),
                new UserPermissionsOptionViewModel(DiagnosticsUserPermissions.View, "View Only")
            };

        public static IEnumerable<UserPermissionsOptionViewModel> UserSystemSettingsPermissionsOptions = new List<UserPermissionsOptionViewModel>()
        {
            new UserPermissionsOptionViewModel(SystemSettingsUserPermissions.None, "Access Denied"),
            new UserPermissionsOptionViewModel(SystemSettingsUserPermissions.View, "View Only"),
            new UserPermissionsOptionViewModel(SystemSettingsUserPermissions.View | SystemSettingsUserPermissions.Edit, "View and Edit")
        };

    }

    public static class UserPermissionsFascade
    {
        public static EditUserPermissionsViewModel BuildUserPermissionsViewModel(PPUserPermissions permissions)
        {
            var model = new EditUserPermissionsViewModel()
            {
                PPUserId = permissions.PPUserId,
                IsAdmin = permissions.IsAdmin,
                UploadPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.FileUploadsUserPermissions.ToString(), UserPermissionsOptionLists.UserUploadsPermissionsOptions),
                SitesPricingPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.SitePricingUserPermissions.ToString(), UserPermissionsOptionLists.UserSitesPricingPermissionsOptions),
                SitesPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.SitesMaintenanceUserPermissions.ToString(), UserPermissionsOptionLists.UserSitesPermissionsOptions),
                ReportsPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.ReportsUserPermissions.ToString(), UserPermissionsOptionLists.UserReportsPermissionsOptions),
                UsersPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.UsersManagementUserPermissions.ToString(), UserPermissionsOptionLists.UserUsersPermissionsOptions),
                DiagnosticsPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.DiagnosticsUserPermissions.ToString(), UserPermissionsOptionLists.UserDiagnosticsPermissionsOptions),
                SystemSettingsPermissionsOptionList = new UserPermissionsOptionListViewModel(permissions.SystemSettingsUserPermissions.ToString(), UserPermissionsOptionLists.UserSystemSettingsPermissionsOptions),
                CreatedOn = permissions.CreatedOn,
                CreatedBy = permissions.CreatedBy,
                UpdatedOn = permissions.UpdatedOn,
                UpdatedBy = permissions.UpdatedBy
            };

            return model;
        }
    }
}