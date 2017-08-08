CREATE PROCEDURE [dbo].[spGetFuelDriveTimeMarkForSiteToCompetitor]
	@FuelTypeId INT,
	@SiteId INT,
	@CompetitorId INT
AS
	SET NOCOUNT ON;

	DECLARE @DriveTime REAL;
	SELECT TOP 1
		@DriveTime = stc.DriveTime
	FROM
		dbo.SiteToCompetitor stc
	WHERE
		stc.SiteId = @SiteId
		AND
		stc.CompetitorId = @CompetitorId;

	DECLARE @DriveTimeMarkup INT;

	SET @DriveTimeMarkup = dbo.fn_GetDriveTimePence(@FuelTypeId, @DriveTime);
	SET @DriveTimeMarkup = COALESCE(@DriveTimeMarkup, 0);

RETURN @DriveTimeMarkup
