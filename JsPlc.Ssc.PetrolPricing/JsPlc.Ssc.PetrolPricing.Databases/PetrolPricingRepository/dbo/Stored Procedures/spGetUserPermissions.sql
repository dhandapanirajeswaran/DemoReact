CREATE PROCEDURE dbo.spGetUserPermissions (
    @PPUserId INT
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		up.[Id],
		up.[PPUserId],
		usr.IsActive [IsActive],
		CASE WHEN usr.IsActive = 1 THEN up.[IsAdmin] ELSE 0 END [IsAdmin],
		CASE WHEN usr.IsActive = 1 THEN up.FileUploadsUserPermissions ELSE 0 END [FileUploadsUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.SitePricingUserPermissions ELSE 0 END [SitePricingUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.SitesMaintenanceUserPermissions ELSE 0 END [SitesMaintenanceUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.ReportsUserPermissions ELSE 0 END [ReportsUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.UsersManagementUserPermissions ELSE 0 END [UsersManagementUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.DiagnosticsUserPermissions ELSE 0 END [DiagnosticsUserPermissions],
		CASE WHEN usr.IsActive = 1 THEN up.SystemSettingsUserPermissions ELSE 0 END [SystemSettingsUserPermissions],
		up.[CreatedOn],
		up.[CreatedBy],
		up.[UpdatedOn],
		up.[UpdatedBy]
	FROM 
		[dbo].[PPUserPermissions] up
		INNER JOIN [dbo].[PPUser] usr ON usr.Id = up.PPUserId
	WHERE
		up.PPUserId = @PPUserId
END