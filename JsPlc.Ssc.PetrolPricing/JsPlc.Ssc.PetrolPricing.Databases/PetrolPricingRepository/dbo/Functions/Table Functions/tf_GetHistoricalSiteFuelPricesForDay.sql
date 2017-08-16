CREATE FUNCTION [dbo].[tf_GetHistoricalSiteFuelPricesForDay]
(
	@ForDate DATE,
	@SiteIds VARCHAR(MAX)
)
RETURNS 
@ResultTV TABLE 
(
	SiteId INT,
	FuelTypeId INT,
	TodayPrice INT,
	PriceSource VARCHAR(20)
)
AS
BEGIN

	--
	-- NOTE: This is a cut-down version of dbo.spCalculateSitePricesForDate
	--

	DECLARE @StartOfToday date = @forDate
	DECLARE @StartOfTomorrow DATE = DATEADD(Day, 1, @StartOfToday)
	DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @StartOfToday)

	DECLARE @Markup_For_Super_Unleaded INT
	SELECT TOP 1
		@Markup_For_Super_Unleaded = ss.SuperUnleadedMarkupPrice
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
			COALESCE(lat.PriceSource, ovr.PriceSource, sug.PriceSource) [PriceSource]
		FROM
			SiteFuels sf
			LEFT JOIN LatestPrices lat ON lat.SiteId = sf.SiteId AND lat.FuelTypeId = sf.FuelTypeId
			LEFT JOIN OverridePrices ovr ON ovr.SiteId = sf.SiteId AND ovr.FuelTypeId = sf.FuelTypeId
			LEFT JOIN SuggestedPrices sug ON sug.SiteId = sf.SiteId AND sug.FuelTypeId = sf.FuelTypeId
	),
	Calculated AS (
		SELECT 
			st.SiteId [SiteId],
			st.FuelTypeId,
			ic.TodayPrice [TodayPrice],
			ic.PriceSource
		FROM
			SiteFuels st
			LEFT JOIN AggregatedTodayPrices ic ON ic.SiteId = st.SiteId AND ic.FuelTypeId = st.FuelTypeId
	),
	AllFuelPrices AS (
		-- UNLEADED and DIESEL
		SELECT
			cal.SiteId,
			cal.FuelTypeId,
			cal.TodayPrice,
			cal.PriceSource
		FROM 
			Calculated cal
		WHERE
			(cal.FuelTypeId = @FuelType_UNLEADED OR cal.FuelTypeId = @FuelType_DIESEL)

		UNION ALL
		-- SUPER UNLEADED when PriceSource = "Latest"
		SELECT
			unl.SiteId,
			sup.FuelTypeId,
			sup.TodayPrice,
			unl.PriceSource
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
			CASE WHEN cal.TodayPrice = 0
				THEN 0
				ELSE cal.TodayPrice + @Markup_For_Super_Unleaded
			END, -- markup Super Unleaded
			super.PriceSource
		FROM 
			Calculated cal
			INNER JOIN Calculated super ON super.SiteId=cal.SiteId and super.FuelTypeId=@FuelType_SUPER_UNLEADED
		WHERE
			cal.FuelTypeId = @FuelType_UNLEADED
			AND
			cal.PriceSource != 'Latest' 
	)

	INSERT INTO @ResultTV
	SELECT
		afp.SiteId [SiteId],
		afp.FuelTypeId [FuelTypeId],
		afp.TodayPrice [TodayPrice], -- Today's
		afp.PriceSource
	FROM 
		AllFuelPrices afp

	RETURN 
END