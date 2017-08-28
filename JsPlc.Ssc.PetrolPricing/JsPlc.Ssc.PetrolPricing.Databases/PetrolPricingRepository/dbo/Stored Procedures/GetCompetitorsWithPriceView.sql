CREATE PROCEDURE [dbo].[GetCompetitorsWithPriceView]
	@ForDate DATE,
	@SiteId INT
AS
BEGIN
SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-08-14'
--DECLARE	@SiteId INT = 993
----DEBUG:END
	
	-- constants
	DECLARE @MaxDriveTime INT = 25

	DECLARE @DayMinus2Date DATE = DATEADD(DAY, -2, @ForDate)
	DECLARE @DayMinus1Date DATE = DATEADD(DAY, -1, @ForDate)

	--
	-- Resultset #1 : Nearby Competitors Names (Active, Within 25 mins and NOT excluded-brand)
	--
	SELECT 
		stc.CompetitorId [SiteId], -- CompetitorId
		stc.SiteId [JsSiteId],
		compsite.CatNo [CatNo],
		compsite.SiteName [StoreName],
		compsite.Brand [Brand],
		compsite.Address [Address],
		stc.DriveTime [DriveTime],
		stc.Distance [Distance],
		compsite.Notes [Notes],
		compsite.IsGrocer [IsGrocer]
	FROM
		dbo.SiteToCompetitor stc
		INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
	WHERE
		stc.SiteId = @SiteId
		AND
		stc.IsExcluded = 0 -- ignore excluded Site Competitors
		AND
		stc.DriveTime < @MaxDriveTime
		AND
		compsite.IsExcludedBrand = 0 -- ignore Excluded Brands
		AND
		compsite.IsActive = 1 -- active Competitor site


	;WITH CompSiteFuels AS (
		SELECT 
			stc.CompetitorId [CompSiteId],
			ft.Id [FuelTypeId]
		FROM
			dbo.SiteToCompetitor stc
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
		WHERE
			stc.SiteId = @SiteId
			AND
			stc.IsExcluded = 0 -- ignore excluded Site Competitors
			AND
			stc.DriveTime < @MaxDriveTime
			AND
			compsite.IsExcludedBrand = 0 -- ignore Excluded Brands
			AND
			compsite.IsActive = 1 -- active Competitor site
	)

	--
	-- Resultset #2 : Nearby Competitors Fuel Prices (Active, Within 25 mins and NOT excluded-brand)
	--
	SELECT
		csf.CompSiteId [SiteId],
		@SiteId [JsSiteId],
		csf.FuelTypeId [FuelTypeId],
		COALESCE(dm1.ModalPrice, 0) [TodayPrice],
		COALESCE(dm2.ModalPrice, 0) [YestPrice],
		CASE WHEN dm1.ModalPrice > 0 AND dm2.ModalPrice > 0
			THEN dm1.ModalPrice - dm2.ModalPrice
			ELSE 0
		END [Difference]
	FROM
		CompSiteFuels csf
		LEFT JOIN dbo.CompetitorPrice dm2 ON dm2.SiteId = csf.CompSiteId AND dm2.FuelTypeId = csf.FuelTypeId AND dm2.DateOfPrice = @DayMinus2Date
		LEFT JOIN dbo.CompetitorPrice dm1 ON dm1.SiteId = csf.CompSiteId AND dm1.FuelTypeId = csf.FuelTypeId AND dm1.DateOfPrice = @DayMinus1Date
END


