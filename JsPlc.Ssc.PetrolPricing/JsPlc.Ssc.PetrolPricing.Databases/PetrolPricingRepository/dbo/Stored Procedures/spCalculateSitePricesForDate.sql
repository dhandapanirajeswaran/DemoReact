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
--SET @SiteIds = '6164'
--DECLARE @PriceSnapshotId INT = NULL
----DEBUG:END

DECLARE @StartOfToday date = @forDate
DECLARE @StartOfTomorrow DATE = DATEADD(Day, 1, @StartOfToday)
DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @StartOfToday)

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
	-- Find 'Tomorrow' Catalist File upload Date
	--
	DECLARE @Tomorrow_DateOfCalc DATE
	SELECT 
		@Tomorrow_DateOfCalc = CONVERT(DATE, MAX(UploadDateTime))
	FROM
		dbo.FileUpload
	WHERE
		StatusId = @ImportProcessStatus_Success
		AND
		UploadTypeId = @UploadType_Daily_Price_Data
		AND
		UploadDateTime < @StartOfTomorrow

	--
	-- Find 'Today' Catalist file upload Date (search backwards to handle weekend/Bank Holidays)
	--
	DECLARE @Today_DateOfCalc DATE
	SELECT 
		@Today_DateOfCalc = CONVERT(DATE, MAX(UploadDateTime))
	FROM
		dbo.FileUpload
	WHERE
		StatusId = @ImportProcessStatus_Success
		AND
		UploadTypeId = @UploadType_Daily_Price_Data
		AND
		UploadDateTime < @Tomorrow_DateOfCalc

	;WITH SiteFuelCombos AS (
		SELECT
			ids.id [SiteId],
			ft.Id [FuelTypeId],
			st.CatNo,
			st.PfsNo,
			st.StoreNo,
			st.PriceMatchType,
			st.CompetitorPriceOffset [TrialPriceMarkup],
			st.CompetitorPriceOffsetNew [MatchCompetitorMarkup]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			INNER JOIN dbo.Site st ON st.Id = ids.Id
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
	)

SELECT
	@PriceSnapshotId [PriceSnapshotId],
	sfc.SiteId [SiteId],
	sfc.FuelTypeId [FuelTypeId],
	COALESCE(tomorrow.SuggestedPrice, 0) [AutoPrice], -- used for Tomorrow
	COALESCE(tomorrow.OverriddenPrice, 0) [OverridePrice], -- used for Tomorrow
	CASE
		WHEN today.OverriddenPrice > 0 THEN today.OverriddenPrice
		WHEN today.SuggestedPrice > 0 THEN today.SuggestedPrice
		ELSE 0
	END [TodayPrice], -- Today's
	tomorrow.Markup [Markup],
	CASE
		WHEN tomorrow.CompetitorId > 0 THEN
			(SELECT TOP 1 Brand + '/' + SiteName FROM dbo.Site WHERE Id = tomorrow.CompetitorId)
		ELSE ''
	END [CompetitorName],
	COALESCE(tomorrow.IsTrailPrice, 0) [IsTrailPrice],
	CASE
		WHEN sfc.PriceMatchType = @PriceMatchType_SoloPrice THEN 0
		WHEN sfc.PriceMatchType = @PriceMatchType_TrailPrice THEN sfc.TrialPriceMarkup
		WHEN sfc.PriceMatchType = @PriceMatchType_MatchCompetitorPrice THEN sfc.MatchCompetitorMarkup
		ELSE 0
	END [CompetitorPriceOffset],
	sfc.PriceMatchType [PriceMatchType],
	CASE
		WHEN tomorrow.OverriddenPrice > 0 THEN 'Override'
		WHEN tomorrow.SuggestedPrice > 0 THEN 'Suggested'
		ELSE ''
	END [PriceSource],
	tomorrow.DateOfCalc [DateOfCalc],
	COALESCE(tomorrow.CompetitorId, 0) [CompetitorSiteId],
	COALESCE(stc.Distance, 0) [Distance],
	COALESCE(stc.DriveTime, 0) [DriveTime],
	dbo.fn_GetDriveTimePence(sfc.FuelTypeId, stc.DriveTime) [DriveTimePence],
	CASE 
		WHEN sfc.PriceMatchType = @PriceMatchType_SoloPrice THEN 0
		WHEN sfc.PriceMatchType = @PriceMatchType_TrailPrice THEN sfc.TrialPriceMarkup
		WHEN sfc.PriceMatchType = @PriceMatchType_MatchCompetitorPrice THEN sfc.MatchCompetitorMarkup
		ELSE 0
	END MatchCompetitorMarkup
FROM 
	SiteFuelCombos sfc
	INNER JOIN dbo.Site st ON st.Id = sfc.SiteId
	LEFT JOIN dbo.SitePrice today ON today.SiteId = sfc.SiteId AND today.FuelTypeId = sfc.FuelTypeId AND today.DateOfCalc = @Today_DateOfCalc
	LEFT JOIN dbo.SitePrice tomorrow ON tomorrow.SiteId = sfc.SiteId AND tomorrow.FuelTypeId = sfc.FuelTypeId AND tomorrow.DateOfCalc = @Tomorrow_DateOfCalc
	LEFT JOIN dbo.SiteToCompetitor stc ON tomorrow.CompetitorId = stc.CompetitorId AND stc.SiteId = sfc.SiteId
ORDER BY 
	SiteId, FuelTypeId
END