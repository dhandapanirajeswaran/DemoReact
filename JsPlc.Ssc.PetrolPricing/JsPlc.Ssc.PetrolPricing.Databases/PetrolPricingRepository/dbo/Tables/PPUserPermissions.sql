CREATE TABLE [dbo].[PPUserPermissions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PPUserId] INT NOT NULL, 
    [IsAdmin] BIT NOT NULL DEFAULT (0), 
    [FileUploadsUserPermissions] INT NOT NULL DEFAULT (0), 
    [SitePricingUserPermissions] INT NOT NULL DEFAULT (0), 
    [SitesMaintenanceUserPermissions] INT NOT NULL DEFAULT (0), 
    [ReportsUserPermissions] INT NOT NULL DEFAULT (0), 
    [UsersManagementUserPermissions] INT NOT NULL DEFAULT (0), 
    [DiagnosticsUserPermissions] INT NOT NULL DEFAULT (0), 
    [CreatedOn] DATETIME NOT NULL, 
    [CreatedBy] INT NOT NULL, 
    [UpdatedOn] DATETIME NOT NULL, 
    [UpdatedBy] INT NOT NULL,
	CONSTRAINT UC_PPUserPermissions_PPUserId UNIQUE(PPUserId)
)
