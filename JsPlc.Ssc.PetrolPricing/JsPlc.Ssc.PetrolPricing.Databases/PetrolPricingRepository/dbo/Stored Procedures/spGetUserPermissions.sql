CREATE PROCEDURE dbo.spGetUserPermissions (
    @PPUserId INT
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		[Id]
		,[PPUserId]
		,[IsAdmin]
		,[FileUploadsUserPermissions]
		,[SitePricingUserPermissions]
		,[SitesMaintenanceUserPermissions]
		,[ReportsUserPermissions]
		,[UsersManagementUserPermissions]
		,[DiagnosticsUserPermissions]
		,[CreatedOn]
		,[CreatedBy]
		,[UpdatedOn]
		,[UpdatedBy]
	FROM 
		[dbo].[PPUserPermissions] up
	WHERE
		up.PPUserId = @PPUserId
END