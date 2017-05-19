﻿CREATE PROCEDURE [dbo].[spCalculateSitePricesForDate] (
	@forDate DATE,
	@SiteIds VARCHAR(MAX)
)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = '2017-05-17'
--DECLARE @SiteIds VARCHAR(MAX) = '9,13,19,21,24,26,53,55,57,58,62,63,64,67,69,71,76,104,110,113,159,162,165,167,206,207,214,256,264,266,275,304,306,309,314,316,321,325,353,355,363,402,412,413,420,421,423,428,429,430,433,507,508,537,539,542,544,547,591,594,598,600,633,636,645,675,676,698,710,733,759,818,820,848,873,905,927,991,994,996,997,999,1000,1002,1040,1051,1053,1077,1078,1083,1085,1087,1097,1127,1128,1134,1160,1163,1167,1168,1169,1173,1201,1204,1211,1215,1223,1251,1259,1262,1263,1265,1268,1269,1274,1302,1304,1305,1313,1345,1349,1350,1395,1396,1403,1405,1437,1439,1440,1441,1494,1495,1503,1559,1560,1593,1599,1629,1637,1653,1696,1710,1740,1741,1748,1749,1750,1754,1793,1838,1847,1853,1854,1856,1895,1896,1910,1934,1976,1993,1995,1996,2037,2098,2105,2106,2107,2108,2137,2138,2141,2142,2148,2177,2180,2181,2222,2267,2309,2338,2389,2391,2395,2399,2426,2429,2447,2448,2449,2476,2491,2494,2604,2627,2732,2735,2756,2762,2791,2795,2798,2837,2849,2874,2923,2959,3047,3082,3084,3104,3117,3157,3180,3186,3187,3189,3192,3355,3374,3409,3417,3420,3421,3461,3513,3514,3543,3544,3551,3569,3603,3716,3735,3800,3801,3816,3895,3907,3925,3953,3954,3955,3969,4053,4174,4210,4227,4278,4319,4366,4367,4368,4425,4473,4526,4536,4565,4588,4654,4735,4813,4866,4919,4986,5014,5015,5037,5147,5148,5149,5155,5195,5254,5288,5471,5478,5716,5794,5796,5810,5910,5945,5957,6140,6177,6178,6179,6180,6181,6182,6183,6184,6185,6186,6187,6188,6189,6190,6191,6192,6193,6194,6195,6196,6197,6198'
----DEBUG:END


DECLARE @StartOfToday date = @forDate
DECLARE @StartOfTomorrow DATE = DATEADD(Day, 1, @StartOfToday)
DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @StartOfToday)

DECLARE @Markup_For_Super_Unleaded INT = 50

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
DECLARE @LatestPrices_FileUploadId INT = 
	(
		SELECT TOP 1 
			fu.Id 
		FROM 
			dbo.FileUpload fu 
		WHERE 
			fu.UploadTypeId = @UploadType_Latest_JS_Price_Data
			AND fu.StatusId = @ImportProcessStatus_Success
			AND fu.UploadDateTime >= @StartOfToday AND fu.UploadDateTime < @StartOfTomorrow
		ORDER BY 
			fu.UploadDateTime DESC
	);

--
-- Find the Latest Catalyst (Daily Price Data) FileUpload Id and UploadDateTime (if any)
--
DECLARE @LatestCatalyst_FileUploadId INT;
DECLARE @LatestCatalyst_UploadDateTime DATETIME

SELECT TOP 1 
	@LatestCatalyst_FileUploadId = fu.Id,
	@LatestCatalyst_UploadDateTime = fu.UploadDateTime
FROM 
	dbo.FileUpload fu
WHERE
	fu.UploadTypeId = @UploadType_Daily_Price_Data
	AND
	fu.StatusId = @ImportProcessStatus_Success
	AND
	fu.UploadDateTime >= @StartOfToday AND fu.UploadDateTime < @StartOfTomorrow
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
		dbo.LatestPrice lp 
		INNER JOIN SiteFuels st ON lp.PfsNo = st.PfsNo and lp.FuelTypeId = st.FuelTypeId
	WHERE
		lp.UploadId = @LatestPrices_FileUploadId
),
Catalist AS (
	SELECT
		dp.*,
		st.Id [SiteId]
	FROM 
		SiteFuels st
		INNER JOIN dbo.DailyPrice dp ON st.CatNo = dp.CatNo AND dp.FuelTypeId = st.FuelTypeId
	WHERE
		dp.DailyUploadId = @LatestCatalyst_FileUploadId
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
		@LatestCatalyst_UploadDateTime [DateOfCalc],
		CONVERT(BIT, 0) [IsTrailPrice],
		'catalyst' [PriceSource]
	FROM
		Catalist cat
	WHERE
		@LatestCatalyst_FileUploadId IS NOT NULL

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
		INNER JOIN dbo.SitePrice sp ON sp.SiteId = st.Id AND sp.FuelTypeId = st.FuelTypeId
	WHERE
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = st.FuelTypeId AND DateOfCalc < @StartOfToday AND OverriddenPrice > 0)
		AND
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
		sp.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sp.SiteId AND FuelTypeId = st.FuelTypeId AND DateOfCalc < @StartOfToday AND SuggestedPrice > 0)
		AND
		sp.FuelTypeId = st.FuelTypeId
		AND
		sp.DateOfCalc < @StartOfToday -- any record BEFORE Today
		AND
		sp.SuggestedPrice > 0	-- search for Suggested (if any)
),

IndexedCalculated AS (
	SELECT ct.*, 
		ROW_NUMBER() OVER (PARTITION BY SiteId, FuelTypeId ORDER BY DateOfCalc DESC) [RowIndex]
	FROM
		CalculatedTemp ct
),
ReducedIndexedCalculated AS (
	SELECT
		ic.*
	FROM
		IndexedCalculated ic
	WHERE
		ic.RowIndex = 1
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
		sp.DateOfCalc >= @StartOfToday AND sp.DateOfCalc < @StartOfTomorrow -- Today
		AND
		sp.SuggestedPrice > 0	-- search for Suggested (if any)
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
		LEFT JOIN ReducedIndexedCalculated ic ON ic.SiteId = st.Id AND ic.FuelTypeId = st.FuelTypeId AND ic.RowIndex = 1
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
		super.SiteId,
		@FuelType_SUPER_UNLEADED [FuelTypeId],
		CASE WHEN cal.AutoPrice = 0
			THEN 0
			ELSE cal.AutoPrice + @Markup_For_Super_Unleaded
		END,
		super.OverridePrice,
		cal.TodayPrice + @Markup_For_Super_Unleaded, -- markup Super Unleaded
		super.Markup,
		super.CompetitorName,
		super.IsTrailPrice,
		super.CompetitorPriceOffset,
		super.PriceMatchType,
		super.PriceSource
	FROM 
		Calculated cal
	    INNER JOIN Calculated super ON super.SiteId=cal.SiteId and super.FuelTypeId=@FuelType_SUPER_UNLEADED
	
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