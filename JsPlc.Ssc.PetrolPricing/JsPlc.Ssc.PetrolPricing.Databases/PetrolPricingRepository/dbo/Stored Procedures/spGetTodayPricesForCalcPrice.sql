CREATE PROCEDURE [dbo].[spGetTodayPricesForCalcPrice] (
	@forDate DATE,
	@SiteId INT
)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = '2017-08-15'
--DECLARE @SiteId INT = 6164
--DECLARE @PriceSnapshotId INT = NULL
----DEBUG:END

DECLARE @StartOfToday date = @forDate
DECLARE @StartOfTomorrow DATE = DATEADD(Day, 1, @StartOfToday)
DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @StartOfToday)

DECLARE @IsHistorical BIT = CASE WHEN CONVERT(DATE, @ForDate) <> CONVERT(DATE, GETDATE()) THEN 1 ELSE 0 END

DECLARE @Markup_For_Super_Unleaded INT
DECLARE @DecimalRounding INT
SELECT TOP 1
	@Markup_For_Super_Unleaded = ss.SuperUnleadedMarkupPrice,
	@DecimalRounding = ss.DecimalRounding
FROM
	dbo.SystemSettings ss


DECLARE @FuelType_SUPER_UNLEADED INT  = 1
DECLARE @FuelType_UNLEADED INT  = 2
DECLARE @FuelType_DIESEL INT  = 6

--
-- dbo.UploadType
--
DECLARE @UploadType_Daily_Price_Data INT = 1
DECLARE @Uploadtype_Quarterly_Site_Date INT = 2
DECLARE @UploadType_Latest_JS_Price_Data INT  = 3
DECLARE @UploadType_Latest_Competitors_Price_Data INT = 4

--
-- dbo.ImportProcessStatus
--
DECLARE @ImportProcessStatus_Uploaded INT = 1
DECLARE @ImportProcessStatus_Warning INT = 2
DECLARE @ImportProcessStatus_Processing INT = 5
DECLARE @ImportProcessStatus_Success INT = 10
DECLARE @ImportProcessStatus_Calculating INT = 11
DECLARE @ImportProcessStatus_CalcFailed INT = 12
DECLARE @ImportProcessStatus_Failed INT = 15
DECLARE @ImportProcessStatus_ImportAborted INT = 16
DECLARE @ImportProcessStatus_CalcAborted INT = 17

--
-- PriceMatchType
--
DECLARE @PriceMatchType_None INT = 0
DECLARE @PriceMatchType_SoloPrice INT = 1
DECLARE @PriceMatchType_TrailPrice INT = 2
DECLARE @PriceMatchType_MatchCompetitorPrice INT = 3

--
-- Get PriceChangeVarianceThreshold value
--
DECLARE @PriceChangeVarianceThreshold INT = (SELECT TOP 1 PriceChangeVarianceThreshold  from dbo.SystemSettings)

DECLARE @LastDailyCatalistUploadDateTime DATETIME;
SELECT TOP 1 
	@LastDailyCatalistUploadDateTime = fu.UploadDateTime 
FROM 
	dbo.FileUpload fu
WHERE
	fu.StatusId = @ImportProcessStatus_Success
	AND
	fu.UploadTypeId = @UploadType_Daily_Price_Data
ORDER BY
	fu.UploadDateTime DESC


--
-- Find the LatestPrices FileUpload Id (if any)
--
DECLARE @LatestPrices_FileUploadId INT
DECLARE @LatestPrices_UploadDateTime DATETIME
 
SELECT TOP 1 
	@LatestPrices_FileUploadId = fu.Id,
	@LatestPrices_UploadDateTime = fu.UploadDateTime
FROM 
	dbo.FileUpload fu 
WHERE 
	fu.UploadTypeId = @UploadType_Latest_JS_Price_Data
	AND fu.StatusId = @ImportProcessStatus_Success

	-- NOTE: look backwards for last upload (in case of weekends/bank holidays)
	AND fu.UploadDateTime < @StartOfTomorrow
ORDER BY 
	fu.UploadDateTime DESC;

--
-- Resultset (lots of CTEs below)
--
;With FuelTypes AS (
	SELECT 
		ft.Id [FuelTypeId]
	FROM 
		dbo.FuelType ft
	WHERE
		ft.Id IN (1, 2, 6)
),
SiteFuelCombo AS (
	SELECT
		st.Id [SiteId],
		st.StoreNo [StoreNo],
		st.TrailPriceCompetitorId [TrailPriceCompetitorId],
		st.PriceMatchType [PriceMatchType],
		st.CompetitorPriceOffset [TrialPriceMarkup],
		st.CompetitorPriceOffsetNew [MatchCompetitorMarkup],
		CASE 
			WHEN st.PriceMatchType = @PriceMatchType_TrailPrice THEN COALESCE(st.CompetitorPriceOffset,  0)
			WHEN st.PriceMatchType = @PriceMatchType_MatchCompetitorPrice THEN COALESCE(st.CompetitorPriceOffsetNew, 0)
			ELSE 0
		END [CompetitorMarkup],
		ft.FuelTypeId [FuelTypeId]
	FROM
		dbo.Site st 
		CROSS APPLY FuelTypes ft
	WHERE
		st.Id = @SiteId
),
AllLatestPrices AS (
	SELECT 
		lp.Id [LatestPriceId],
		lp.ModalPrice,
		lp.FuelTypeId,
		lp.StoreNo,
		lp.PfsNo,
		sfc.SiteId
	FROM 
		SiteFuelCombo sfc
		INNER JOIN DBO.LatestPrice lp ON lp.StoreNo = sfc.StoreNo and lp.FuelTypeId = sfc.FuelTypeId
	WHERE
		lp.UploadId = @LatestPrices_FileUploadId
),
LatestPrices AS (
	-- ============= CASE 1: LATEST PRICE FOR "TODAY" (finds the most recent latest price per site per fuel)
	SELECT
		lp.ModalPrice [TodayPrice],		-- latestprice
		lp.FuelTypeId,
		lp.SiteId,
		@LatestPrices_UploadDateTime [DateOfCalc], -- Latest Price ALWAYS at the top !
		CONVERT(BIT, 0) [IsTrailPrice],
		'Latest' [PriceSource],
		@LatestPrices_UploadDateTime [PriceSourceDateTime]
	FROM 
		SiteFuelCombo sfc
		INNER JOIN AllLatestPrices lp ON lp.SiteId = sfc.SiteId AND lp.FuelTypeId = sfc.FuelTypeId
	WHERE
		lp.LatestPriceId = (SELECT MAX(LatestPriceId) FROM AllLatestPrices WHERE SiteId = sfc.SiteId AND FuelTypeId = sfc.FuelTypeId)
),
OverridePrices AS (
	-- ============= CASE 3: YESTERDAY'S OVERRIDE FOR "TODAY" COLUMN
	SELECT
		sp.OverriddenPrice [TodayPrice],
		sp.FuelTypeId,
		sp.SiteId,
		CONVERT(DATETIME, @StartOfToday) [DateOfCalc],
		sp.IsTrailPrice,
		'Override' [PriceSource],
		sp.DateOfCalc [PriceSourceDateTime]
	FROM
		SiteFuelCombo sfc
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = sfc.SiteId AND sp.FuelTypeId = sfc.FuelTypeId
	WHERE
		NOT EXISTS(SELECT TOP 1 NULL FROM LatestPrices WHERE SiteId = sfc.SiteId AND FuelTypeId = sfc.FuelTypeId)
		AND
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = sfc.FuelTypeId AND DateOfCalc < @StartOfToday AND OverriddenPrice > 0)
),
SuggestedPrices AS (
	-- ============= CASE 3: YESTERDAY'S SUGGESTED FOR "TODAY" COLUMN
	SELECT
		sp.SuggestedPrice [TodayPrice],
		sp.FuelTypeId,
		sp.SiteId,
		CONVERT(DATETIME, @StartOfToday) [DateOfCalc],
		sp.IsTrailPrice,
		'Suggested' [PriceSource],
		sp.DateOfCalc [PriceSourceDateTime]
	FROM
		SiteFuelCombo sfc
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = sfc.SiteId AND sp.FuelTypeId = sfc.FuelTypeId
	WHERE
		NOT EXISTS(SELECT TOP 1 NULL FROM LatestPrices WHERE SiteId = sfc.SiteId AND FuelTypeId = sfc.FuelTypeId)
		AND
		NOT EXISTS(SELECT TOP 1 NULL FROM OverridePrices WHERE SiteId = sfc.SiteId AND FuelTypeId = sfc.FuelTypeId)
		AND
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = sfc.FuelTypeId AND DateOfCalc < @StartOfToday AND SuggestedPrice > 0)
),
AggregatedTodayPrices as (
	SELECT
		COALESCE(lat.TodayPrice, ovr.TodayPrice, sug.TodayPrice) [TodayPrice],
		sfc.FuelTypeId,
		sfc.SiteId,
		COALESCE(lat.DateOfCalc, ovr.DateOfCalc, sug.DateOfCalc) [DateOfCalc],
		COALESCE(lat.IsTrailPrice, ovr.IsTrailPrice, sug.IsTrailPrice) [IsTrailPrice],
		COALESCE(lat.PriceSource, ovr.PriceSource, sug.PriceSource) [PriceSource],
		COALESCE(lat.PriceSourceDateTime, ovr.PriceSourceDateTime, sug.PriceSourceDateTime) [PriceSourceDateTime]
	FROM
		SiteFuelCombo sfc
		LEFT JOIN LatestPrices lat ON lat.SiteId = sfc.SiteId AND lat.FuelTypeId = sfc.FuelTypeId
		LEFT JOIN OverridePrices ovr ON ovr.SiteId = sfc.SiteId AND ovr.FuelTypeId = sfc.FuelTypeId
		LEFT JOIN SuggestedPrices sug ON sug.SiteId = sfc.SiteId AND sug.FuelTypeId = sfc.FuelTypeId
),
TomorrowPrices AS (
	SELECT
		CASE sfc.PriceMatchType 
			WHEN 1 THEN sp.SuggestedPrice+sp.Markup -- standard/solo price
			WHEN 2 THEN sp.SuggestedPrice -- Trial price
			WHEN 3 THEN sp.SuggestedPrice+sp.Markup -- Match Competitor
			ELSE sp.SuggestedPrice+sp.Markup
		END [TodaySuggestedPrice],
		sp.OverriddenPrice [TodayOverridePrice],
		sp.FuelTypeId,
		sp.SiteId,
		sp.CompetitorId,
		sp.Markup
	FROM
		SiteFuelCombo sfc
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = sfc.SiteId
	WHERE
		sp.Id = (
			SELECT TOP 1 
				Id 
			FROM dbo.SitePrice 
			WHERE SiteId = sfc.SiteId 
				AND FuelTypeId = sfc.FuelTypeId 
				--AND DateOfCalc >= @StartOfToday AND DateOfCalc < @StartOfTomorrow -- Today
				AND DateOfCalc < @StartOfTomorrow -- Today
				AND (SuggestedPrice > 0 OR OverriddenPrice > 0) -- NOTE: SuggestedPrice can be 0 and OverriddenPrice != 0
			ORDER BY DateOfCalc DESC
			)
),
Calculated AS (
	SELECT 
		sfc.SiteId [SiteId],
		sfc.FuelTypeId,
		COALESCE(tp.TodaySuggestedPrice, 0) [AutoPrice],
		COALESCE(tp.TodayOverridePrice, 0) [OverridePrice],
		atp.TodayPrice [TodayPrice],
		CASE 
			WHEN sfc.PriceMatchType = @PriceMatchType_TrailPrice THEN COALESCE(sfc.TrialPriceMarkup,  0)
			WHEN sfc.PriceMatchType = @PriceMatchType_MatchCompetitorPrice THEN COALESCE(sfc.MatchCompetitorMarkup, 0)
			ELSE COALESCE(tp.Markup, 0)
		END [Markup],
		CASE 
			WHEN sfc.TrailPriceCompetitorId IS NOT NULL THEN COALESCE(matchcomp.Brand + '/' + matchcomp.SiteName, '') -- Match Competitor Price Match ?
			ELSE COALESCE(compsite.Brand + '/' + compsite.SiteName, '') -- Else cheapest Competitor
		END [CompetitorName],
		CASE 
			WHEN sfc.TrailPriceCompetitorId IS NOT NULL THEN matchcomp.Id
			ELSE compsite.id
		END [CompetitorSiteId],
		atp.IsTrailPrice [IsTrailPrice],
		sfc.CompetitorMarkup [CompetitorPriceOffset],
		sfc.PriceMatchType [PriceMatchType],
		atp.PriceSource,
		atp.PriceSourceDateTime,
		sfc.MatchCompetitorMarkup
	FROM
		SiteFuelCombo sfc
		LEFT JOIN dbo.Site matchcomp ON matchcomp.Id = sfc.TrailPriceCompetitorId
		LEFT JOIN AggregatedTodayPrices atp ON atp.SiteId = sfc.SiteId AND atp.FuelTypeId = sfc.FuelTypeId
		LEFT JOIN TomorrowPrices tp ON tp.SiteId = sfc.SiteId AND tp.FuelTypeId = sfc.FuelTypeId
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
		cal.PriceSource,
		cal.PriceSourceDateTime,
		cal.CompetitorSiteId,
		cal.MatchCompetitorMarkup
	FROM 
		Calculated cal
	WHERE
		(cal.FuelTypeId = @FuelType_UNLEADED OR cal.FuelTypeId = @FuelType_DIESEL)

	UNION ALL
	-- SUPER UNLEADED when PriceSource = "Latest"
	SELECT
		unleaded.SiteId,
		sup.FuelTypeId,
		CASE WHEN unleaded.AutoPrice = 0 THEN 0
			ELSE unleaded.AutoPrice + @Markup_For_Super_Unleaded
		END,
		sup.OverridePrice,
		sup.TodayPrice,
		unleaded.Markup,
		unleaded.CompetitorName,
		unleaded.IsTrailPrice,
		unleaded.CompetitorPriceOffset,
		unleaded.PriceMatchType,
		unleaded.PriceSource,
		unleaded.PriceSourceDateTime,
		unleaded.CompetitorSiteId,
		unleaded.MatchCompetitorMarkup
	FROM 
		Calculated sup
		INNER JOIN Calculated unleaded ON unleaded.SiteId = sup.SiteId AND unleaded.FuelTypeId = @FuelType_UNLEADED
	WHERE
		sup.FuelTypeId = @FuelType_SUPER_UNLEADED 
		AND 
		sup.PriceSource = 'Latest'
		
	UNION ALL
	-- SUPER UNLEADED when Price Source is NOT 'Latest'
	SELECT
		super.SiteId,
		@FuelType_SUPER_UNLEADED [FuelTypeId],
		CASE WHEN unleaded.AutoPrice = 0
			THEN 0
			ELSE unleaded.AutoPrice + @Markup_For_Super_Unleaded
		END,
		COALESCE(super.OverridePrice, 0), -- Override for Super-Unleaded
		CASE WHEN unleaded.TodayPrice = 0
			THEN 0
			ELSE unleaded.TodayPrice + @Markup_For_Super_Unleaded
		END, -- markup Super Unleaded
		super.Markup,
		unleaded.CompetitorName,
		unleaded.IsTrailPrice,
		unleaded.CompetitorPriceOffset,
		unleaded.PriceMatchType,
		unleaded.PriceSource,
		unleaded.PriceSourceDateTime,
		unleaded.CompetitorSiteId,
		unleaded.MatchCompetitorMarkup
	FROM 
		Calculated unleaded
	    INNER JOIN Calculated super ON super.SiteId = unleaded.SiteId and super.FuelTypeId = @FuelType_SUPER_UNLEADED
	WHERE
		unleaded.FuelTypeId = @FuelType_UNLEADED
		AND
		(
			unleaded.PriceSource IS NULL
			OR
			unleaded.PriceSource != 'Latest'  
		)
)
SELECT
	NULL [PriceSnapshotId],
	afp.SiteId [SiteId],
	afp.FuelTypeId [FuelTypeId],
	afp.AutoPrice [AutoPrice], -- used for Tomorrow
	afp.OverridePrice [OverridePrice], -- used for Tomorrow
	COALESCE(afp.TodayPrice,0) [TodayPrice], -- Today's
	afp.Markup [Markup],
	afp.CompetitorName [CompetitorName],
	COALESCE(afp.IsTrailPrice, 0) [IsTrailPrice],
	afp.CompetitorPriceOffset [CompetitorPriceOffset],
	afp.PriceMatchType [PriceMatchType],
	COALESCE(afp.PriceSource, 'Catalist') [PriceSource],
	COALESCE(afp.PriceSourceDateTime, @LastDailyCatalistUploadDateTime) [PriceSourceDateTime],
	COALESCE(afp.CompetitorSiteId, 0) [CompetitorSiteId],
	COALESCE(stc.Distance, 0) [Distance],
	COALESCE(stc.DriveTime, 0) [DriveTime],
	dbo.fn_GetDriveTimePence(afp.FuelTypeId, stc.DriveTime) [DriveTimePence],
	afp.MatchCompetitorMarkup
FROM 
	AllFuelPrices afp
	LEFT JOIN dbo.SiteToCompetitor stc ON stc.CompetitorId = afp.CompetitorSiteId AND stc.SiteId = afp.SiteId
ORDER BY 
	SiteId, FuelTypeId
END