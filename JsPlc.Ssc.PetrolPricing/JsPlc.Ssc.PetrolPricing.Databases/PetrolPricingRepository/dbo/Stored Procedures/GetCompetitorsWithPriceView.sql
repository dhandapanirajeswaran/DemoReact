CREATE PROCEDURE [dbo].[GetCompetitorsWithPriceView]
	@ForDate DATE,
	@SiteId INT,
	@SiteIds VARCHAR(MAX) = NULL
AS
BEGIN
SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-09-03'
--DECLARE	@SiteId INT = 3425
--DECLARE @SiteIds VARCHAR(MAX) ='6164,9'
----DEBUG:END
	
	-- handle single Site
	IF @SiteIds IS NULL
	BEGIN
		SET @SiteIds = CONVERT(VARCHAR(MAX), @SiteId)
	END

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
		compsite.IsGrocer [IsGrocer],
		compsite.IsExcludedBrand [IsExcludedBrand],
		stc.IsExcluded [IsExcluded],
		compsite.IsActive [IsActive]
	FROM
		dbo.tf_SplitIdsOnComma(@SiteIds) ids
		INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = ids.Id
		INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
	WHERE
		stc.DriveTime < @MaxDriveTime

		-- NOTE: Excluded Site Competiors, Excluded Brands or Inactive Competitor sites are handled via UI
		--		This allows historical competitor prices to be seen


	;WITH CompSiteFuels AS (
		SELECT 
			stc.CompetitorId [CompSiteId],
			ids.Id [JsSiteId],
			ft.Id [FuelTypeId],
			compsite.IsSainsburysSite [IsSainsburysSite]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = ids.Id
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
		WHERE
			stc.DriveTime < @MaxDriveTime

		-- NOTE: Excluded Site Competiors, Excluded Brands or Inactive Competitor sites are handled via UI
		--		This allows historical competitor prices to be seen
	)

	--
	-- Resultset #2 : Nearby Competitors Fuel Prices (Active, Within 25 mins and NOT excluded-brand)
	--
	,MergedCompetitorPrices AS ( 
		SELECT
			csf.CompSiteId [SiteId],
			csf.JsSiteId [JsSiteId],
			csf.FuelTypeId [FuelTypeId],
			CASE 
				WHEN csf.IsSainsburysSite = 1 
				THEN 
					CASE WHEN js1.OverriddenPrice > 0 THEN js1.OverriddenPrice ELSE js1.SuggestedPrice END
				ELSE
					COALESCE(dm1.ModalPrice, 0)
			END [TodayPrice],
			CASE
				WHEN csf.IsSainsburysSite = 1
				THEN
					CASE WHEN js2.OverriddenPrice > 0 THEN js2.OverriddenPrice ELSE js2.SuggestedPrice END
				ELSE
					COALESCE(dm2.ModalPrice, 0)
			END [YestPrice]

			---- debug
			--,csf.CompSiteId
		FROM
			CompSiteFuels csf
			LEFT JOIN dbo.CompetitorPrice dm2 ON dm2.SiteId = csf.CompSiteId AND dm2.FuelTypeId = csf.FuelTypeId AND dm2.DateOfPrice = @DayMinus2Date
			LEFT JOIN dbo.CompetitorPrice dm1 ON dm1.SiteId = csf.CompSiteId AND dm1.FuelTypeId = csf.FuelTypeId AND dm1.DateOfPrice = @DayMinus1Date
			-- NOTE: needs to handle Sainsburys and Non-Sainsburys competitors
			LEFT JOIN dbo.SitePrice js2 ON js2.SiteId = csf.CompSiteId AND js2.FuelTypeId = csf.FuelTypeId AND js2.DateOfCalc = @DayMinus2Date
			LEFT JOIN dbo.SitePrice js1 ON js1.SiteId = csf.CompSiteId AND js1.FuelTypeId = csf.FuelTypeId AND js1.DateOfCalc = @DayMinus1Date
	)
	SELECT
		mcp.SiteId [SiteId],
		mcp.JsSiteId [JsSiteId],
		mcp.FuelTypeId [FuelTypeId],
		COALESCE(mcp.TodayPrice, 0) [TodayPrice],
		COALESCE(mcp.YestPrice, 0) [YestPrice],
		CASE 
			WHEN mcp.TodayPrice > 0 AND mcp.YestPrice > 0
			THEN mcp.TodayPrice - mcp.YestPrice
			ELSE 0
		END [Difference]

		---- debug
		--,(select top 1 sitename from dbo.Site where Id = mcp.CompSiteId)
	FROM
		MergedCompetitorPrices mcp

END

