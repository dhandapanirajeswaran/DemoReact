using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System.Xml;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions
{
    public class UserAccessViewModel
    {
        public bool IsUserAuthenticated { get; set; }
        public bool IsActive { get; set; }
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
            this.IsActive = false;
            this.PPUserId = 0;
            this.UserName = "";
            this.IsAdmin = false;
        }
    }

    public class UserFileUploadsAccess
    {
        public FileUploadsUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(FileUploadsUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanUpload
        {
            get { return this.Permissions.HasFlag(FileUploadsUserPermissions.Upload); }
        }
        public UserFileUploadsAccess(FileUploadsUserPermissions permissions = FileUploadsUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }

    public class UserSitePricingAccess
    {
        public SitesPricingUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(SitesPricingUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanExport
        {
            get { return this.Permissions.HasFlag(SitesPricingUserPermissions.Export); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanUpdate
        {
            get { return this.Permissions.HasFlag(SitesPricingUserPermissions.Update); }
        }

        public UserSitePricingAccess(SitesPricingUserPermissions permissions = SitesPricingUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }

    public class UserSitesMaintenanceAccess
    {
        public SitesMaintenanceUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(SitesMaintenanceUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanAdd
        {
            get { return this.Permissions.HasFlag(SitesMaintenanceUserPermissions.Add); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanEdit
        {
            get { return this.Permissions.HasFlag(SitesMaintenanceUserPermissions.Edit); }
        }

        public UserSitesMaintenanceAccess(SitesMaintenanceUserPermissions permissions = SitesMaintenanceUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }

    public class UserReportsAccess
    {
        public ReportsUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(ReportsUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanExport
        {
            get { return this.Permissions.HasFlag(ReportsUserPermissions.Export); }
        }

        public UserReportsAccess(ReportsUserPermissions permissions = ReportsUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }

    public class UserUserManagementAccess
    {
        public UsersManagementUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(UsersManagementUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanAdd
        {
            get { return this.Permissions.HasFlag(UsersManagementUserPermissions.Add); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanEdit
        {
            get { return this.Permissions.HasFlag(UsersManagementUserPermissions.Edit); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanDelete
        {
            get { return this.Permissions.HasFlag(UsersManagementUserPermissions.Delete); }
        }

        public UserUserManagementAccess(UsersManagementUserPermissions permissions = UsersManagementUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }

    public class UserDiagnosticsAccess
    {
        public DiagnosticsUserPermissions Permissions { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanView
        {
            get { return this.Permissions.HasFlag(DiagnosticsUserPermissions.View); }
        }

        [System.Xml.Serialization.XmlIgnore]
        public bool CanEdit
        {
            get { return this.Permissions.HasFlag(DiagnosticsUserPermissions.Edit); }
        }

        public UserDiagnosticsAccess(DiagnosticsUserPermissions permissions = DiagnosticsUserPermissions.None)
        {
            this.Permissions = permissions;
        }
    }
}