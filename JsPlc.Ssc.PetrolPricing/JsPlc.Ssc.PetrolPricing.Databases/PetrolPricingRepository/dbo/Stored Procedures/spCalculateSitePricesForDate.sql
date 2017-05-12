CREATE PROCEDURE [dbo].[spCalculateSitePricesForDate] (
	@forDate DATE,
	@SiteIds VARCHAR(MAX)
)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = '2017-05-10'
--DECLARE @SiteIds VARCHAR(MAX) = '6188,9'
----DEBUG:END


DECLARE @StartOfToday date = @forDate
DECLARE @StartOfTomorrow DATE = DATEADD(Day, 1, @StartOfToday)
DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @StartOfToday)

DECLARE @Markup_For_Super_Unleaded INT = 50

DECLARE @FuelType_SUPER_UNLEADED INT  = 1
DECLARE @FuelType_UNLEADED INT  = 2
DECLARE @FuelType_DIESEL INT  = 6

;With OurSites AS (
	SELECT
		st.*
	FROM
		dbo.tf_SplitIdsOnComma(@SiteIds) ids
		INNER JOIN dbo.Site st ON st.Id = ids.Id
),
FuelTypes AS (
	SELECT @FuelType_SUPER_UNLEADED [FuelTypeId]
	UNION ALL
	SELECT @FuelType_UNLEADED [FuelTypeId]
	UNION ALL
	SELECT @FuelType_DIESEL [FuelTypeId]
),
SiteFuels AS (
	SELECT
		os.*,
		ft.FuelTypeId
	FROM
		OurSites os
		CROSS APPLY FuelTypes ft
),
latestPrices AS (
	SELECT 
		lp.ModalPrice,
		lp.FuelTypeId,
		lp.StoreNo,
		lp.PfsNo,
		st.Id [SiteId]
	FROM 
		SiteFuels st
		INNER JOIN dbo.LatestPrice lp ON lp.PfsNo = st.PfsNo
		INNER JOIN dbo.FileUpload fu ON fu.Id = lp.UploadId
	WHERE
		fu.UploadDateTime >= @StartOfToday AND fu.UploadDateTime < @StartOfTomorrow
		AND 
		lp.FuelTypeId = st.FuelTypeId
),
Catalist AS (
	SELECT
		dp.*,
		st.Id [SiteId],
		fu.UploadDateTime
	FROM 
		SiteFuels st
		INNER JOIN dbo.DailyPrice dp ON st.CatNo = dp.CatNo
		INNER JOIN dbo.FileUpload fu ON fu.Id = dp.DailyUploadId
	WHERE
		fu.UploadDateTime >= @StartOfToday AND fu.UploadDateTime < @StartOfTomorrow
		AND
		dp.FuelTypeId = st.FuelTypeId
)
,
CalculatedTemp AS (
	-- ============= CASE 1: LATEST PRICE FOR "TODAY"
	SELECT
		lp.ModalPrice [TodayPrice],		-- latestprice
		lp.FuelTypeId,
		lp.SiteId,
		'3000-01-01' [DateOfCalc], -- Latest Price ALWAYS at the top !
		CONVERT(BIT, 0) [IsTrailPrice],
		'latest' [PriceSource]
	FROM 
		latestPrices lp

	UNION ALL
	-- ============= CASE 2: CATALYST PRICE FOR "TODAY"
	SELECT 
		cat.ModalPrice [TodayPrice],
		cat.FuelTypeId,
		cat.SiteId,
		cat.UploadDateTime [DateOfCalc],
		CONVERT(BIT, 0) [IsTrailPrice],
		'catalyst' [PriceSource]
	FROM
		Catalist cat

	UNION ALL
	-- ============= CASE 3: YESTERDAY'S OVERRIDE FOR "TODAY" COLUMN
	SELECT
		sp.OverriddenPrice [TodayPrice],
		sp.FuelTypeId,
		sp.SiteId,
		DATEADD(SECOND, 1, CONVERT(DATETIME, @StartOfToday)) [DateOfCalc],
		sp.IsTrailPrice,
		'Override' [PriceSource]
	FROM
		SiteFuels st
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.Id
	WHERE
		sp.FuelTypeId = st.FuelTypeId
		AND
		sp.OverriddenPrice > 0 -- search for Override (if any)
		AND
		sp.DateOfCalc < @StartOfToday -- any record BEFORE Today

	UNION ALL
	-- ============= CASE 3: YESTERDAY'S SUGGESTED FOR "TODAY" COLUMN
	SELECT
		sp.SuggestedPrice [TodayPrice],
		sp.FuelTypeId,
		sp.SiteId,
		--sp.DateOfCalc,
		@StartOfToday [DateOfCalc],
		sp.IsTrailPrice,
		'Suggested' [PriceSource]
	FROM
		SiteFuels st 
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.Id
	WHERE
		sp.FuelTypeId = st.FuelTypeId
		AND
		sp.SuggestedPrice > 0	-- search for Suggested (if any)
		AND
		sp.DateOfCalc < @StartOfToday -- any record BEFORE Today
),

IndexedCalculated AS (
	SELECT ct.*, 
		ROW_NUMBER() OVER (PARTITION BY SiteId, FuelTypeId ORDER BY DateOfCalc DESC) [RowIndex]
	FROM
		CalculatedTemp ct
),
TomorrowPrices AS (
	SELECT
		sp.SuggestedPrice+sp.Markup [TodaySuggestedPrice],
		sp.OverriddenPrice [TodayOverridePrice],
		sp.FuelTypeId,
		sp.SiteId,
		sp.CompetitorId,
		sp.Markup
	FROM
		SiteFuels st 
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.Id
	WHERE
		sp.FuelTypeId = st.FuelTypeId
		AND
		sp.SuggestedPrice > 0	-- search for Suggested (if any)
		AND
		sp.DateOfCalc >= @StartOfToday AND sp.DateOfCalc < @StartOfTomorrow -- Today
),

Calculated AS (
	SELECT 
		st.Id [SiteId],
		st.FuelTypeId,
		COALESCE(tp.TodaySuggestedPrice, 0) [AutoPrice],
		COALESCE(tp.TodayOverridePrice, 0) [OverridePrice],
		ic.TodayPrice [TodayPrice],
		COALESCE(tp.Markup, 0) [Markup],

		CASE WHEN st.TrailPriceCompetitorId IS NOT NULL
			THEN COALESCE(matchcomp.Brand + '/' + matchcomp.SiteName, '') -- Match Competitor Price Match ?
			ELSE COALESCE(compsite.Brand + '/' + compsite.SiteName, '') -- Else best Competitor
		END [CompetitorName],
		ic.IsTrailPrice [IsTrailPrice],
		st.CompetitorPriceOffset [CompetitorPriceOffset],
		st.PriceMatchType [PriceMatchType],
		ic.PriceSource
	FROM
		SiteFuels st
		LEFT JOIN dbo.Site matchcomp ON matchcomp.Id = st.TrailPriceCompetitorId
		LEFT JOIN IndexedCalculated ic ON ic.SiteId = st.Id AND ic.FuelTypeId = st.FuelTypeId AND ic.RowIndex = 1
		LEFT JOIN TomorrowPrices tp ON tp.SiteId = st.Id AND tp.FuelTypeId = st.FuelTypeId
		LEFT JOIN dbo.Site compsite ON compsite.Id = tp.CompetitorId
),

AllFuelPrices AS (
	-- UNLEADED and DIESEL
	SELECT
		cal.SiteId,
		cal.FuelTypeId,
		cal.AutoPrice,
		cal.OverridePrice,
		cal.TodayPrice,
		cal.Markup,
		cal.CompetitorName,
		cal.IsTrailPrice,
		cal.CompetitorPriceOffset,
		cal.PriceMatchType,
		cal.PriceSource
	FROM 
		Calculated cal
	WHERE
		(cal.FuelTypeId = @FuelType_UNLEADED OR cal.FuelTypeId = @FuelType_DIESEL)

	UNION ALL
	-- SUPER UNLEADED when PriceSource = "Latest"
	SELECT
		cal.SiteId,
		cal.FuelTypeId,
		cal.AutoPrice,
		cal.OverridePrice,
		cal.TodayPrice,
		cal.Markup,
		cal.CompetitorName,
		cal.IsTrailPrice,
		cal.CompetitorPriceOffset,
		cal.PriceMatchType,
		cal.PriceSource
	FROM 
		Calculated cal
	WHERE
		(cal.FuelTypeId = @FuelType_SUPER_UNLEADED AND cal.PriceSource = 'latest') -- Super Unleaded
	UNION ALL
	-- SUPER UNLEADED when Price Source is NOT 'latest'
	SELECT
		cal.SiteId,
		@FuelType_SUPER_UNLEADED [FuelTypeId],
		cal.AutoPrice,
		cal.OverridePrice,
		cal.TodayPrice + @Markup_For_Super_Unleaded, -- markup Super Unleaded
		cal.Markup,
		cal.CompetitorName,
		cal.IsTrailPrice,
		cal.CompetitorPriceOffset,
		cal.PriceMatchType,
		cal.PriceSource
	FROM 
		Calculated cal
	WHERE
		cal.FuelTypeId = @FuelType_UNLEADED
		AND
		cal.PriceSource != 'latest' 

)
SELECT
	afp.SiteId [SiteId],
	afp.FuelTypeId [FuelTypeId],
	afp.AutoPrice [AutoPrice], -- used for Tomorrow
	afp.OverridePrice [OverridePrice], -- used for Tomorrow
	afp.TodayPrice [TodayPrice], -- Today's
	afp.Markup [Markup],
	afp.CompetitorName [CompetitorName],
	afp.IsTrailPrice [IsTrailPrice],
	afp.CompetitorPriceOffset [CompetitorPriceOffset],
	afp.PriceMatchType [PriceMatchType],
	afp.PriceSource
FROM 
	AllFuelPrices afp
ORDER BY 
	SiteId, FuelTypeId
END