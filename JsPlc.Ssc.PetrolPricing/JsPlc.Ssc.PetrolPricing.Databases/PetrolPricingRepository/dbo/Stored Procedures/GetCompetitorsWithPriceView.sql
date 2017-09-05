CREATE PROCEDURE [dbo].[GetCompetitorsWithPriceView]
	@ForDate DATE,
	@SiteId INT,
	@SiteIds VARCHAR(MAX) = NULL
AS
BEGIN
SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-08-29'
--DECLARE	@SiteId INT = 13
--DECLARE @SiteIds VARCHAR(MAX) = null
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
	DECLARE @DayMinus0Date DATE = DATEADD(DAY, 0, @ForDate)

	DECLARE @DayMinus1CompetitorPriceDate DATE = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE DateOfPrice <= @DayMinus1Date);
	DECLARE @DayMinus2CompetitorPriceDate DATE = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE DateOfPrice <= @DayMinus2Date);

	-- NOTE: Sainsburys dbo.SitePrice.DateOfCalc are for Tomorrow but have today's date (real Date = dbo.SitePrice.DateOfCalc + 1 Day !)
	DECLARE @DayMinus1SitePriceDate DATE = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE DateOfCalc <= @DayMinus1Date);
	DECLARE @DayMinus2SitePriceDate DATE = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE DateOfCalc <= @DayMinus2Date);

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
	,LastNonSainsburysPrices AS (
		SELECT
			csf.CompSiteId, 
			csf.JsSiteId,
			csf.FuelTypeId,
			COALESCE(dm1.ModalPrice, 0) [TodayPrice],
			dm1.DateOfPrice [TodayDate],
			COALESCE(dm2.ModalPrice, 0) [YestPrice],
			dm2.DateOfPrice [YestDate]
		FROM
			CompSiteFuels csf
			LEFT JOIN dbo.CompetitorPrice dm1 ON dm1.SiteId = csf.CompSiteId AND dm1.FuelTypeId = csf.FuelTypeId AND dm1.DateOfPrice = @DayMinus1CompetitorPriceDate
			LEFT JOIN dbo.CompetitorPrice dm2 ON dm2.SiteId = csf.CompSiteId AND dm2.FuelTypeId = csf.FuelTypeId AND dm2.DateOfPrice = @DayMinus2CompetitorPriceDate
		WHERE
			csf.IsSainsburysSite = 0
	),
	LastSainsburysPrices AS (
		SELECT
			csf.CompSiteId,
			csf.JsSiteId,
			csf.FuelTypeId,
			CASE
				WHEN js1.OverriddenPrice > 0 THEN js1.OverriddenPrice - (js1.DriveTimeMarkup * 10) -- Remove the Drive-Time markup
				WHEN js1.SuggestedPrice > 0 THEN js1.SuggestedPrice - (js1.DriveTimeMarkup * 10) -- Remove the Drive-Time markup
				ELSE 0
			END [TodayPrice],
			DATEADD(DAY, 1, js1.DateOfCalc) [TodayDate],
			CASE
				WHEN js2.OverriddenPrice > 0 THEN js2.OverriddenPrice - (js2.DriveTimeMarkup * 10) -- Remove the Drive-Time markup
				WHEN js2.SuggestedPrice > 0 THEN js2.SuggestedPrice - (js2.DriveTimeMarkup * 10) -- Remove the Drive-Time markup
				ELSE 0
			END [YestPrice],
			DATEADD(DAY, 1, js2.DateOfCalc) [YestDate]

		FROM
			CompSiteFuels csf
			LEFT JOIN dbo.SitePrice js1 ON js1.SiteId = csf.CompSiteId AND js1.FuelTypeId = csf.FuelTypeId AND js1.DateOfCalc = @DayMinus1SitePriceDate
			LEFT JOIN dbo.SitePrice js2 ON js2.SiteId = csf.CompSiteId AND js2.FuelTypeId = csf.FuelTypeId AND js2.DateOfCalc = @DayMinus2SitePriceDate

		WHERE
			csf.IsSainsburysSite = 1
	)
	,MergedCompetitorPrices AS ( 
		SELECT
			lnsp.CompSiteId,
			lnsp.JsSiteId,
			lnsp.FuelTypeId,
			lnsp.TodayPrice,
			lnsp.TodayDate,
			lnsp.YestPrice,
			lnsp.YestDate
		FROM
			LastNonSainsburysPrices lnsp
		UNION ALL
		SELECT
			lsp.CompSiteId,
			lsp.JsSiteId,
			lsp.FuelTypeId,
			lsp.TodayPrice,
			lsp.TodayDate,
			lsp.YestPrice,
			lsp.YestDate
		FROM
			LastSainsburysPrices lsp
	)
	--
	-- Resultset #2 : Nearby Competitors Fuel Prices (Active, Within 25 mins and NOT excluded-brand)
	--
	SELECT
		csf.CompSiteId [SiteId],
		csf.JsSiteId [JsSiteId],
		csf.FuelTypeId [FuelTypeId],
		COALESCE(mcp.YestPrice, 0) [YestPrice],
		mcp.YestDate [YestDate],
		COALESCE(mcp.TodayPrice, 0) [TodayPrice],
		mcp.TodayDate [TodayDate],
		CASE 
			WHEN mcp.TodayPrice > 0 AND mcp.YestPrice > 0
			THEN mcp.TodayPrice - mcp.YestPrice
			ELSE 0
		END [Difference],
		csf.IsSainsburysSite [IsSainsburysSite]

		---- debug
		--,(select top 1 sitename from dbo.Site where Id = csf.CompSiteId)
	FROM
		CompSiteFuels csf
		LEFT JOIN MergedCompetitorPrices mcp ON mcp.CompSiteId = csf.CompSiteId AND mcp.FuelTypeId = csf.FuelTypeId
END

