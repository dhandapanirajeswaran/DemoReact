CREATE PROCEDURE [dbo].[spCalculateSitePricesForDate] (
	@forDate DATE,
	@SiteIds VARCHAR(MAX),
	@PriceSnapshotId INT = NULL
)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = GETDATE()
--DECLARE @SiteIds VARCHAR(MAX) = '9,13,19,21,24,26,53,55,57,58,62,63,64,67,69,71,76,104,110,113,159,162,165,167,206,207,214,256,264,266,275,304,306,309,314,316,321,325,353,355,363,402,412,413,420,421,423,428,429,430,433,507,508,537,539,542,544,547,591,594,598,600,633,636,645,675,676,698,710,733,759,818,820,848,873,905,927,991,994,996,997,999,1000,1002,1040,1051,1053,1077,1078,1083,1085,1087,1097,1127,1128,1134,1160,1163,1167,1168,1169,1173,1201,1204,1211,1215,1223,1251,1259,1262,1263,1265,1268,1269,1274,1302,1304,1305,1313,1345,1349,1350,1395,1396,1403,1405,1437,1439,1440,1441,1494,1495,1503,1559,1560,1593,1599,1629,1637,1653,1696,1710,1740,1741,1748,1749,1750,1754,1793,1838,1847,1853,1854,1856,1895,1896,1910,1934,1976,1993,1995,1996,2037,2098,2105,2106,2107,2108,2137,2138,2141,2142,2148,2177,2180,2181,2222,2267,2309,2338,2389,2391,2395,2399,2426,2429,2447,2448,2449,2476,2491,2494,2604,2627,2732,2735,2756,2762,2791,2795,2798,2837,2849,2874,2923,2959,3047,3082,3084,3104,3117,3157,3180,3186,3187,3189,3192,3355,3374,3409,3417,3420,3421,3461,3513,3514,3543,3544,3551,3569,3603,3716,3735,3800,3801,3816,3895,3907,3925,3953,3954,3955,3969,4053,4174,4210,4227,4278,4319,4366,4367,4368,4425,4473,4526,4536,4565,4588,4654,4735,4813,4866,4919,4986,5014,5015,5037,5147,5148,5149,5155,5195,5254,5288,5471,5478,5716,5794,5796,5810,5910,5945,5957,6140,6177,6178,6179,6180,6181,6182,6183,6184,6185,6186,6187,6188,6189,6190,6191,6192,6193,6194,6195,6196,6197,6198'
--SET @SiteIds = '992'
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

--
-- Fuel types
--
DECLARE @FuelTypesTV TABLE (FuelTypeId INT PRIMARY KEY, FuelName VARCHAR(20))
INSERT INTO @FuelTypesTV (FuelTypeId, FuelName)
VALUES (@FuelType_SUPER_UNLEADED, 'Super Unleaded'),
		(@FuelType_UNLEADED, 'Unleaded'),
		(@FuelType_DIESEL, 'Diesel')

--
-- List of Sainsburys Sites
--
DECLARE @SainsburysSitesTV TABLE (SiteId INT PRIMARY KEY)
INSERT INTO @SainsburysSitesTV (SiteId)
SELECT DISTINCT Id [SiteId]
FROM
	dbo.tf_SplitIdsOnComma(@SiteIds)

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
;With OurSites AS (
	SELECT
		st.*
	FROM
		@SainsburysSitesTV ids
		INNER JOIN dbo.Site st ON st.Id = ids.SiteId
),
FuelTypes AS (
	SELECT 
		tv.FuelTypeId, tv.FuelName
	FROM 
		@FuelTypesTV tv
),
SiteFuels AS (
	SELECT
		os.Id [SiteId],
		os.StoreNo [StoreNo],
		os.TrailPriceCompetitorId [TrailPriceCompetitorId],
		os.PriceMatchType [PriceMatchType],
		os.CompetitorPriceOffset [CompetitorPriceOffset],
		ft.FuelTypeId [FuelTypeId]
	FROM
		OurSites os
		CROSS APPLY FuelTypes ft
),
AllLatestPrices AS (
	SELECT 
		lp.Id [LatestPriceId],
		lp.ModalPrice,
		lp.FuelTypeId,
		lp.StoreNo,
		lp.PfsNo,
		st.SiteId
	FROM 
		SiteFuels st
		INNER JOIN DBO.LatestPrice lp ON lp.StoreNo = st.StoreNo and lp.FuelTypeId = st.FuelTypeId
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
		SiteFuels sf
		INNER JOIN AllLatestPrices lp ON lp.SiteId = sf.SiteId AND lp.FuelTypeId = sf.FuelTypeId
	WHERE
		lp.LatestPriceId = (SELECT MAX(LatestPriceId) FROM AllLatestPrices WHERE SiteId = sf.SiteId AND FuelTypeId = sf.FuelTypeId)
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
		SiteFuels st
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.SiteId AND sp.FuelTypeId = st.FuelTypeId
	WHERE
		NOT EXISTS(SELECT TOP 1 NULL FROM LatestPrices WHERE SiteId = st.SiteId AND FuelTypeId = st.FuelTypeId)
		AND
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = st.FuelTypeId AND DateOfCalc < @StartOfToday AND OverriddenPrice > 0)
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
		SiteFuels st
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.SiteId AND sp.FuelTypeId = st.FuelTypeId
	WHERE
		NOT EXISTS(SELECT TOP 1 NULL FROM LatestPrices WHERE SiteId = st.SiteId AND FuelTypeId = st.FuelTypeId)
		AND
		NOT EXISTS(SELECT TOP 1 NULL FROM OverridePrices WHERE SiteId = st.SiteId AND FuelTypeId = st.FuelTypeId)
		AND
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = st.FuelTypeId AND DateOfCalc < @StartOfToday AND SuggestedPrice > 0)
),
AggregatedTodayPrices as (
	SELECT
		COALESCE(lat.TodayPrice, ovr.TodayPrice, sug.TodayPrice) [TodayPrice],
		sf.FuelTypeId,
		sf.SiteId,
		COALESCE(lat.DateOfCalc, ovr.DateOfCalc, sug.DateOfCalc) [DateOfCalc],
		COALESCE(lat.IsTrailPrice, ovr.IsTrailPrice, sug.IsTrailPrice) [IsTrailPrice],
		COALESCE(lat.PriceSource, ovr.PriceSource, sug.PriceSource) [PriceSource],
		COALESCE(lat.PriceSourceDateTime, ovr.PriceSourceDateTime, sug.PriceSourceDateTime) [PriceSourceDateTime]
	FROM
		SiteFuels sf
		LEFT JOIN LatestPrices lat ON lat.SiteId = sf.SiteId AND lat.FuelTypeId = sf.FuelTypeId
		LEFT JOIN OverridePrices ovr ON ovr.SiteId = sf.SiteId AND ovr.FuelTypeId = sf.FuelTypeId
		LEFT JOIN SuggestedPrices sug ON sug.SiteId = sf.SiteId AND sug.FuelTypeId = sf.FuelTypeId
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
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.SiteId
	WHERE
		sp.Id = (
			SELECT TOP 1 
				Id 
			FROM dbo.SitePrice 
			WHERE SiteId = st.SiteId 
				AND FuelTypeId = st.FuelTypeId 
				AND DateOfCalc >= @StartOfToday AND DateOfCalc < @StartOfTomorrow -- Today
				AND (SuggestedPrice > 0 OR OverriddenPrice > 0) -- NOTE: SuggestedPrice can be 0 and OverriddenPrice != 0
			ORDER BY DateOfCalc DESC
			)
),
Calculated AS (
	SELECT 
		st.SiteId [SiteId],
		st.FuelTypeId,
		COALESCE(tp.TodaySuggestedPrice, 0) [AutoPrice],
		COALESCE(tp.TodayOverridePrice, 0) [OverridePrice],
		ic.TodayPrice [TodayPrice],

		CASE WHEN st.PriceMatchType = @PriceMatchType_TrailPrice
			THEN COALESCE(tp.Markup, st.CompetitorPriceOffset, 0)
			ELSE COALESCE(tp.Markup, 0)
		END [Markup],
		CASE WHEN st.TrailPriceCompetitorId IS NOT NULL
			THEN COALESCE(matchcomp.Brand + '/' + matchcomp.SiteName, '') -- Match Competitor Price Match ?
			ELSE COALESCE(compsite.Brand + '/' + compsite.SiteName, '') -- Else best Competitor
		END [CompetitorName],
		CASE WHEN st.TrailPriceCompetitorId IS NOT NULL
			THEN matchcomp.Id
			ELSE compsite.id
		END [CompetitorSiteId],
		ic.IsTrailPrice [IsTrailPrice],
		st.CompetitorPriceOffset [CompetitorPriceOffset],
		st.PriceMatchType [PriceMatchType],
		ic.PriceSource,
		ic.PriceSourceDateTime
	FROM
		SiteFuels st
		LEFT JOIN dbo.Site matchcomp ON matchcomp.Id = st.TrailPriceCompetitorId
		LEFT JOIN AggregatedTodayPrices ic ON ic.SiteId = st.SiteId AND ic.FuelTypeId = st.FuelTypeId
		LEFT JOIN TomorrowPrices tp ON tp.SiteId = st.SiteId AND tp.FuelTypeId = st.FuelTypeId
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
		cal.CompetitorSiteId
	FROM 
		Calculated cal
	WHERE
		(cal.FuelTypeId = @FuelType_UNLEADED OR cal.FuelTypeId = @FuelType_DIESEL)

	UNION ALL
	-- SUPER UNLEADED when PriceSource = "Latest"
	SELECT
		unl.SiteId,
		sup.FuelTypeId,
		CASE WHEN unl.AutoPrice = 0 THEN 0
			ELSE unl.AutoPrice + @Markup_For_Super_Unleaded
		END,
		sup.OverridePrice,
		sup.TodayPrice,
		unl.Markup,
		unl.CompetitorName,
		unl.IsTrailPrice,
		unl.CompetitorPriceOffset,
		unl.PriceMatchType,
		unl.PriceSource,
		unl.PriceSourceDateTime,
		unl.CompetitorSiteId
	FROM 
		Calculated sup
		INNER JOIN Calculated unl ON unl.SiteId = sup.SiteId AND unl.FuelTypeId = @FuelType_UNLEADED
	WHERE
		sup.FuelTypeId = @FuelType_SUPER_UNLEADED 
		AND 
		sup.PriceSource = 'Latest'

		-- (unl.FuelTypeId = @FuelType_SUPER_UNLEADED AND unl.PriceSource = 'Latest') -- Super Unleaded
	UNION ALL
	-- SUPER UNLEADED when Price Source is NOT 'Latest'
	SELECT
		super.SiteId,
		@FuelType_SUPER_UNLEADED [FuelTypeId],
		CASE WHEN cal.AutoPrice = 0
			THEN 0
			ELSE cal.AutoPrice + @Markup_For_Super_Unleaded
		END,
		COALESCE(super.OverridePrice, 0),
		CASE WHEN cal.TodayPrice = 0
			THEN 0
			ELSE cal.TodayPrice + @Markup_For_Super_Unleaded
		END, -- markup Super Unleaded
		super.Markup,
		super.CompetitorName,
		super.IsTrailPrice,
		super.CompetitorPriceOffset,
		super.PriceMatchType,
		COALESCE(super.PriceSource, cal.PriceSource),
		COALESCE(super.PriceSourceDateTime, cal.PriceSourceDateTime),
		super.CompetitorSiteId
	FROM 
		Calculated cal
	    INNER JOIN Calculated super ON super.SiteId=cal.SiteId and super.FuelTypeId=@FuelType_SUPER_UNLEADED
	WHERE
		cal.FuelTypeId = @FuelType_UNLEADED
		AND
		cal.PriceSource != 'Latest' 
)
SELECT
	@PriceSnapshotId [PriceSnapshotId],
	afp.SiteId [SiteId],
	afp.FuelTypeId [FuelTypeId],
	CASE 
		WHEN @IsHistorical = 1 THEN afp.AutoPrice -- don't change value for Historical data
		ELSE dbo.fn_ReplaceLastPriceDigit(
			dbo.fn_RoundTomorrowPriceByPriceChangeVariance(@PriceChangeVarianceThreshold, afp.TodayPrice, afp.AutoPrice),
			@DecimalRounding)
	END [AutoPrice], -- used for Tomorrow
	afp.OverridePrice [OverridePrice], -- used for Tomorrow
	afp.TodayPrice [TodayPrice], -- Today's
	afp.Markup [Markup],
	afp.CompetitorName [CompetitorName],
	COALESCE(afp.IsTrailPrice, 0) [IsTrailPrice],
	afp.CompetitorPriceOffset [CompetitorPriceOffset],
	afp.PriceMatchType [PriceMatchType],
	afp.PriceSource,
	afp.PriceSourceDateTime,
	COALESCE(afp.CompetitorSiteId, 0) [CompetitorSiteId],
	COALESCE(stc.Distance, 0) [Distance],
	COALESCE(stc.DriveTime, 0) [DriveTime],
	dbo.fn_GetDriveTimePence(afp.FuelTypeId, stc.DriveTime) [DriveTimePence]
FROM 
	AllFuelPrices afp
	LEFT JOIN dbo.SiteToCompetitor stc ON stc.CompetitorId = afp.CompetitorSiteId AND stc.SiteId = afp.SiteId
ORDER BY 
	SiteId, FuelTypeId
END