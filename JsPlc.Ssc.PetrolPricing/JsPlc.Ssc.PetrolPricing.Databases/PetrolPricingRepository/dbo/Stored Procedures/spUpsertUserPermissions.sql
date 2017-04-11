CREATE PROCEDURE dbo.spUpsertUserPermissions (
    @PPUserId INT,
    @IsAdmin BIT,
    @FileUploadsUserPermissions INT,
    @SitePricingUserPermissions INT,
    @SitesMaintenanceUserPermissions INT,
    @ReportsUserPermissions INT,
    @UsersManagementUserPermissions INT,
    @DiagnosticsUserPermissions INT,
	@RequestingPPUserId INT
)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT NULL FROM dbo.PPUserPermissions WHERE PPUserId = @PPUserId)
	BEGIN
		UPDATE
			dbo.PPUserPermissions
		SET
			FileUploadsUserPermissions = @FileUploadsUserPermissions,
			SitePricingUserPermissions = @SitePricingUserPermissions,
			SitesMaintenanceUserPermissions = @SitesMaintenanceUserPermissions,
			ReportsUserPermissions = @ReportsUserPermissions,
			UsersManagementUserPermissions = @UsersManagementUserPermissions,
			DiagnosticsUserPermissions = @DiagnosticsUserPermissions,
			UpdatedOn = GetDate(),
			UpdatedBy = @RequestingPPUserId
		WHERE
			PPUserId = @PPUserId
	END
	ELSE
	BEGIN
		INSERT INTO
			dbo.PPUserPermissions
			(
				PPUserId, 
				IsAdmin, 
				FileUploadsUserPermissions, 
				SitePricingUserPermissions, 
				SitesMaintenanceUserPermissions, 
				ReportsUserPermissions, 
				UsersManagementUserPermissions, 
				DiagnosticsUserPermissions, 
				CreatedOn, 
				CreatedBy, 
				UpdatedOn, 
				UpdatedBy
			)
		VALUES
			(
				@PPUserId, 
				0, 
				@FileUploadsUserPermissions, 
				@SitePricingUserPermissions,
				@SitesMaintenanceUserPermissions,
				@ReportsUserPermissions,
				@UsersManagementUserPermissions,
				@DiagnosticsUserPermissions,
				GetDate(),
				@RequestingPPUserId,
				GetDate(),
				@RequestingPPUserId
			)
	END
	RETURN 0
END
