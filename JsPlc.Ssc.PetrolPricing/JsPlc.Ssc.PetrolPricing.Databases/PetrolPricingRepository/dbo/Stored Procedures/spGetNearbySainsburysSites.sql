CREATE PROCEDURE [dbo].[spGetNearbySainsburysSites]
	 @SiteId INT
AS
BEGIN
	SELECT
		compsite.Id [CompetitorSiteId],
		compsite.Brand [Brand],
		compsite.SiteName [SiteName],
		compsite.Town [Town],
		compsite.PostCode [PostCode],
		compsite.IsActive [IsActive],
		stc.Distance [Distance],
		stc.DriveTime [DriveTime],
		dbo.fn_GetDriveTimePence(2, stc.DriveTime) [UnleadedDriveTimeMarkup],
		dbo.fn_GetDriveTimePence(6, stc.DriveTime) [DieselDriveTimeMarkup],
		dbo.fn_GetDriveTimePence(1, stc.DriveTime) [SuperUnleadedDriveTimeMarkup]
	FROM
		dbo.SiteToCompetitor stc
		INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
	WHERE
		stc.SiteId = @SiteId
		AND
		compsite.IsSainsburysSite = 1
	ORDER BY
		stc.Distance

END