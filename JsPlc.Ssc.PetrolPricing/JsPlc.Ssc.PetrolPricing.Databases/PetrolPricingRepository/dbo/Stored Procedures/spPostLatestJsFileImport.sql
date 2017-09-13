CREATE PROCEDURE [dbo].[spPostLatestJsFileImport]
	@FileUploadId INT,
	@FileUploadDateTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @FileUploadId INT = 8
--DECLARE @FileUploadDateTime DATETIME = '2017-09-12 16:11:36.000'
----DEBUG:END


	DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @FileUploadDateTime);
	DECLARE @IsPriceFreeze BIT = dbo.fn_IsPriceFreezeActiveForDate(@FileUploadDateTime);

	--
	-- Constants
	--
	DECLARE @PriceReasonFlags_CheapestPriceFound INT = 0x00000001
    DECLARE @PriceReasonFlags_Rounded INT = 0x00000002
	DECLARE @PriceReasonFlags_InsidePriceVariance INT = 0x00000004
	DECLARE @PriceReasonFlags_OutsidePriceVariance INT = 0x00000008

	DECLARE @PriceReasonFlags_TodayPriceSnapBack INT = 0x00000010
    DECLARE @PriceReasonFlags_HasGrocers INT = 0x00000020
	DECLARE @PriceReasonFlags_HasIncompleteGrocers INT = 0x00000040
	DECLARE @PriceReasonFlags_BasedOnUnleaded INT = 0x00000080

    DECLARE @PriceReasonFlags_MissingSiteCatNo INT = 0x00000100
    DECLARE @PriceReasonFlags_MissingDailyCatalist INT = 0x00000200
	DECLARE @PriceReasonFlags_NoMatchCompetitorPrice INT = 0x00000400
	DECLARE @PriceReasonFlags_NoSuggestedPrice INT = 0x00000800

	DECLARE @PriceReasonFlags_PriceStuntFreeze INT = 0x00001000
	DECLARE @PriceReasonFlags_LatestJSPrice INT = 0x00002000
	DECLARE @PriceReasonFlags_ManualOverride INT = 0x00004000
	DECLARE @PriceReasonFlags_MatchCompetitorFound INT = 0x00008000

	DECLARE @PriceReasonFlags_TrialPriceFound INT = 0x00010000

	--
	-- Get Latest JS Prices and Today Prices per Site per Fuel
	--
	;WITH LatestTodaySitePrices AS (
		SELECT
			st.Id [SiteId],
			lp.FuelTypeId [FuelTypeId],
			lp.ModalPrice [LatestJsPrice],
			dbo.fn_TodayPriceForSiteFuelDate(st.Id, lp.FuelTypeId, @FileUploadDateTime) [TodayPrice]
		FROM
			dbo.LatestPrice lp
			INNER JOIN dbo.Site st ON st.PfsNo = lp.PfsNo AND st.StoreNo = lp.StoreNo
		WHERE
			lp.ModalPrice > 0
	)
	--
	-- Determine if Price Snapback (due to Price Freeze cap)
	--
	,TodayPricesWithSnapback AS (
		SELECT
			ltsp.SiteId,
			ltsp.FuelTypeId,
			ltsp.LatestJsPrice,
			ltsp.TodayPrice,
			CASE 
				WHEN @IsPriceFreeze = 1 AND ltsp.TodayPrice > 0 AND ltsp.LatestJsPrice > ltsp.TodayPrice 
				THEN 1
				ELSE 0
			END [IsPriceSnapback]
		FROM
			LatestTodaySitePrices ltsp
	)
	--
	-- Determine new Today Price and Price Reason
	--
	,NewTodayPricesWithReasons AS (
		SELECT
			tpws.SiteId,
			tpws.FuelTypeId,
			CASE 
				WHEN tpws.IsPriceSnapback = 1 THEN tpws.TodayPrice 
				ELSE tpws.LatestJsPrice
			END [NewTodayPrice],
			CASE
				WHEN tpws.IsPriceSnapback = 1 THEN @PriceReasonFlags_PriceStuntFreeze + @PriceReasonFlags_TodayPriceSnapBack
				ELSE @PriceReasonFlags_LatestJSPrice
			END [PriceReasonFlags]
		FROM
			TodayPricesWithSnapback tpws
	)

	MERGE dbo.SitePrice AS target
	USING (
		SELECT
			ntp.SiteId,
			ntp.FuelTypeId,
			ntp.NewTodayPrice,
			ntp.PriceReasonFlags
		FROM
			NewTodayPricesWithReasons ntp
	) AS source(SiteId, FuelTypeId, NewTodayPrice, PriceReasonFlags)
	ON (target.SiteId = source.SiteId AND target.FuelTypeId = source.FuelTypeId AND target.DateOfCalc = @StartOfYesterday)
	WHEN MATCHED THEN
		UPDATE SET
			target.SuggestedPrice = source.NewTodayPrice,
			target.OverriddenPrice = 0,
			target.CompetitorId = NULL,
			target.Markup = 0,
			target.IsTrailPrice = 0,
			target.PriceReasonFlags = source.PriceReasonFlags,
			target.DriveTimeMarkup = 0,
			target.CompetitorCount = 0,
			target.CompetitorPriceCount = 0,
			target.GrocerCount = 0,
			target.GrocerPriceCount = 0,
			target.NearbyGrocerCount = 0,
			target.NearbyGrocerPriceCount = 0
	WHEN NOT MATCHED BY target THEN
	INSERT (
		SiteId,
		FuelTypeId,
		DateOfCalc,
		DateOfPrice,
		UploadId,
		EffDate,
		SuggestedPrice,
		OverriddenPrice,
		CompetitorId,
		Markup,
		IsTrailPrice,
		PriceReasonFlags,
		DriveTimeMarkup,
		CompetitorCount,
		CompetitorPriceCount,
		GrocerCount,
		GrocerPriceCount,
		NearbyGrocerCount,
		NearbyGrocerPriceCount
	) VALUES (
		source.SiteId,
		source.FuelTypeId,
		@StartOfYesterday,
		@FileUploadDateTime,
		@FileUploadId,
		NULL,
		source.NewTodayPrice,
		0,
		NULL,
		0,
		0,
		source.PriceReasonFlags,
		0,
		0,
		0,
		0,
		0,
		0,
		0
	);

END
