CREATE FUNCTION [dbo].[tf_FindCheapestPrice]
(
	@SiteId INT,
	@FuelTypeId INT,
	@ForDate DATE,
	@FileUploadId INT,
	@MaxDriveTime INT
)
RETURNS 
@Result TABLE 
(
	SiteId INT,
	FuelTypeId INT,
	DateOfCalc DATE,
	DateOfPrice DATE,
	SuggestedPrice INT,
	UploadId INT,
	Markup INT,
	CompetitorId INT,
	IsTrialPrice BIT,
	IsTodayPrice BIT,
	PriceReasonFlags INT
)
AS
BEGIN

----DEBUG:START
--DECLARE	@SiteId INT = 9
--DECLARE	@FuelTypeId INT = 2
--DECLARE	@ForDate DATE = '2017-08-14 12:30:00'
--DECLARE	@FileUploadId INT = 3
--DECLARE	@MaxDriveTime INT = 25

--DECLARE @Result TABLE 
--(
--	SiteId INT,
--	FuelTypeId INT,
--	DateOfCalc DATE,
--	DateOfPrice DATE,
--	SuggestedPrice INT,
--	UploadId INT,
--	Markup INT,
--	CompetitorId INT,
--	IsTrialPrice BIT,
--	IsTodayPrice BIT,
--	PriceReasonFlags INT
--)
----DEBUG:END

	DECLARE @PriceStuntFreeze BIT = 0

	DECLARE @StartOfYesterday DATETIME = CONVERT(DATE, DATEADD(DAY, -1, @ForDate))
	DECLARE @StartOfToday DATETIME = CONVERT(DATE, @ForDate)
	DECLARE @StartOfTomorrow DATETIME = DATEADD(DAY, 1, @StartOfToday)

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

	--
	-- Get SystemSettings
	--
	DECLARE @SystemSettings_MaxGrocerDriveTime INT
	DECLARE @SystemSettings_PriceVariance INT
	DECLARE @SystemSettings_DecimalRounding INT

	SELECT TOP 1 
		@SystemSettings_MaxGrocerDriveTime = MaxGrocerDriveTimeMinutes,
		@SystemSettings_PriceVariance = ss.PriceChangeVarianceThreshold,
		@SystemSettings_DecimalRounding = ss.DecimalRounding
	FROM 
		dbo.SystemSettings ss

	--
	-- result
	--
	DECLARE @Cheapest_SiteId INT = @SiteId;
	DECLARE @Cheapest_FuelTypeId INT = @FuelTypeId;
	DECLARE @Cheapest_DateOfCalc DATE = CONVERT(DATE, @ForDate)
	DECLARE @Cheapest_DateOfPrice DATE = CONVERT(DATE, DATEADD(DAY, -1, @ForDate))
	DECLARE @Cheapest_SuggestedPrice INT = 0
	DECLARE @Cheapest_UploadId INT = @FileUploadId
	DECLARE @Cheapest_Markup INT = 0
	DECLARE @Cheapest_CompetitorId INT = NULL
	DECLARE @Cheapest_IsTrialPrice BIT = 0
	DECLARE @Cheapest_IsTodayPrice BIT = 0
	DECLARE @Cheapest_PriceReasonFlags INT = 0

	DECLARE @MinPriceFound INT = NULL
	DECLARE @MinPriceCompetitorId INT = NULL

	--
	-- Lookup Site information
	--
	DECLARE @Site_CatNo INT
	DECLARE @Site_PfsNo INT
	DECLARE @Site_StoreNo INT
	DECLARE @Site_PriceMatchType INT
	DECLARE @Site_MatchCompetitorSiteId INT
	DECLARE @Site_MatchCompetitorMarkup INT
	DECLARE @Site_TrialPriceMarkup INT

	SELECT TOP 1
		@Site_CatNo = COALESCE(st.CatNo, 0),
		@Site_PfsNo = COALESCE(st.PfsNo, 0),
		@Site_StoreNo = COALESCE(st.StoreNo, 0),
		@Site_PriceMatchType = st.PriceMatchType,
		@Site_MatchCompetitorSiteId = CASE WHEN st.PriceMatchType = 3 THEN st.TrailPriceCompetitorId ELSE NULL END,
		@Site_MatchCompetitorMarkup = CASE WHEN st.PriceMatchType = 3 THEN COALESCE(st.CompetitorPriceOffsetNew,0) ELSE 0 END,
		@Site_TrialPriceMarkup = CASE WHEN st.PriceMatchType = 2 THEN COALESCE(st.CompetitorPriceOffset, 0) ELSE 0 END
	FROM
		dbo.Site st
	WHERE
		st.Id = @SiteId
	--
	-- Check if Daily Catalist file exists for Date	
	--
	DECLARE @IsDailyCatalistFileForDate BIT = CASE 
		WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId IN (5, 10, 11) AND UploadTypeId = 1 AND UploadDateTime BETWEEN @StartOfToday AND @StartOfTomorrow)
		THEN 1
		ELSE 0
	END

	--
	-- Check for Latest JS Price Data for Date
	--
	DECLARE @LatestJsPrice_FileUploadId INT
	SET @LatestJsPrice_FileUploadId = dbo.fn_LastFileUploadForDate(@ForDate, 3)

	-- lookup Latest Site Fuel Price from Latest JS Price Data
	DECLARE @LatestJsPrice_ModalPrice INT = NULL
	DECLARE @LatestJsPrice_Id INT = NULL
	DECLARE @LatestJsPrice_UploadDateTime DATETIME
	SELECT TOP 1
		@LatestJsPrice_ModalPrice = lp.ModalPrice,
		@LatestJsPrice_Id = lp.Id,
		@LatestJsPrice_UploadDateTime = (SELECT TOP 1 UploadDateTime FROM dbo.FileUpload WHERE Id = @LatestJsPrice_FileUploadId)
	FROM
		dbo.LatestPrice lp
	WHERE
		lp.FuelTypeId = @FuelTypeId
		AND
		lp.PfsNo = @Site_PfsNo
		AND
		lp.StoreNo = @Site_StoreNo
		AND
		lp.UploadId = @LatestJsPrice_FileUploadId

	IF @Site_CatNo = 0
		SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_MissingSiteCatNo

	IF @IsDailyCatalistFileForDate = 0
		SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_MissingDailyCatalist

	--
	-- ensure Site has a CatNo and a Daily Catalist file exists for date
	--
	IF @Site_CatNo > 0 AND @IsDailyCatalistFileForDate = 1
	BEGIN

		--
		-- Is there Latest JS Price available for Site Fuel ?
		--
		IF @LatestJsPrice_ModalPrice IS NOT NULL
		BEGIN
			SET @Cheapest_DateOfPrice = CONVERT(DATE, @LatestJsPrice_UploadDateTime)
			SET @Cheapest_SuggestedPrice = @LatestJsPrice_ModalPrice
			SET @Cheapest_UploadId = @LatestJsPrice_FileUploadId
			SET @Cheapest_Markup = 0
			SET @Cheapest_CompetitorId = NULL
			SET @Cheapest_IsTrialPrice = 0
			SET @Cheapest_IsTodayPrice = 0
			SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_LatestJSPrice
		END

		--
		-- Price Strategy Match: Match Competitor ?
		--
		IF @Site_PriceMatchType = 3 AND @LatestJsPrice_ModalPrice IS NULL
		BEGIN
			--
			-- Search for most recent Competitor Site Price for Fuel on (or before) Date
			--
			SELECT TOP 1
				@MinPriceFound = cp.ModalPrice,
				@MinPriceCompetitorId = cp.SiteId
			FROM
				dbo.CompetitorPrice cp
			WHERE
				cp.Id = (SELECT TOP 1 Id 
					FROM dbo.CompetitorPrice 
					WHERE 
						SiteId = @Site_MatchCompetitorSiteId 
						AND FuelTypeId = @FuelTypeId
						AND ModalPrice > 0 
						AND DateOfPrice = @StartOfYesterday
					)

			IF @MinPriceFound > 0
			BEGIN
				SET @Cheapest_SuggestedPrice = @MinPriceFound + @Site_MatchCompetitorMarkup * 10
				SET @Cheapest_CompetitorId = @MinPriceCompetitorId
				SET @Cheapest_Markup = @Site_MatchCompetitorMarkup
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_CheapestPriceFound
			END
			ELSE
			BEGIN
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_NoMatchCompetitorPrice
			END
		END

		--
		-- Price Strategy Match: Standard Price OR Trial price ?
		--
		IF (@Site_PriceMatchType = 1 OR @Site_PriceMatchType = 2) AND @LatestJsPrice_ModalPrice IS NULL
		BEGIN
			--
			-- Find the cheapest 0-25 min Competitor
			--	
			;WITH NearbyActiveCompetitorPrices as (
				SELECT
					stc.CompetitorId,
					stc.Distance,
					stc.DriveTime,
					cp.ModalPrice,
					dbo.fn_GetDriveTimePence(@FuelTypeId, stc.DriveTime) * 10 [DriveTimeMarkup],
					cp.DailyPriceId,
					cp.LatestCompPriceId
				FROM
					dbo.SiteToCompetitor stc
					INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
					INNER JOIN dbo.CompetitorPrice cp ON cp.Id = (
						SELECT TOP 1
							Id
						FROM
							dbo.CompetitorPrice
						WHERE
							SiteId = stc.CompetitorId
							AND
							FuelTypeId = @FuelTypeId
							AND
							DateOfPrice = @StartOfYesterday
						ORDER BY
							DateOfPrice DESC
					)
				WHERE
					stc.SiteId = @SiteId
					AND
					stc.IsExcluded = 0 -- Site Competitor is not excluded
					AND 
					compsite.IsActive = 1 -- Competitor site is active
					AND
					compsite.IsExcludedBrand = 0 -- Not an Excluded Brand
					AND
					compsite.IsActive = 1 -- Active Competitor
					AND
					stc.DriveTime < @MaxDriveTime
				),
				NearbyCompetitorPricesIncDriveTime AS (
					SELECT
						nacp.*,
						nacp.ModalPrice + nacp.DriveTimeMarkup [PriceIncDriveTime]
					FROM
						NearbyActiveCompetitorPrices nacp
				),
				BestCompetitorPrice AS (
					SELECT
						ncp.*
					FROM
						NearbyCompetitorPricesIncDriveTime ncp
					WHERE
						ncp.PriceIncDriveTime = (SELECT MIN(PriceIncDriveTime) from NearbyCompetitorPricesIncDriveTime)
				)
				SELECT TOP 1
					@Cheapest_CompetitorId = bcp.CompetitorId,
					@Cheapest_SuggestedPrice = bcp.PriceIncDriveTime + @Site_TrialPriceMarkup * 10,
					@Cheapest_Markup = @Site_TrialPriceMarkup,
					@Cheapest_DateOfPrice = 
					CASE 
						WHEN bcp.DailyPriceId IS NOT NULL
						THEN dp.DateOfPrice
						WHEN bcp.LatestCompPriceId IS NOT NULL
						THEN (SELECT TOP 1 UploadDateTime FROM dbo.FileUpload WHERE Id = lcp.UploadId)
						ELSE ''
					END,
					@Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_CheapestPriceFound
				FROM 
					BestCompetitorPrice bcp
					LEFT JOIN dbo.DailyPrice dp ON dp.Id = bcp.DailyPriceId
					LEFT JOIN dbo.LatestCompPrice lcp ON lcp.Id = bcp.LatestCompPriceId

				--
				-- Get most recent 'Today' price for Site Fuel (if any)
				--
				DECLARE @Today_Price INT
				SELECT
					@Today_Price = CASE
						WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
						WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
						ELSE 0
					END
				FROM
					dbo.SitePrice sp
				WHERE
					sp.Id = (
						SELECT TOP 1
							Id
						FROM
							dbo.SitePrice
						WHERE
							SiteId = @SiteId
							AND
							FuelTypeId = @FuelTypeId
							AND
							DateOfCalc < @StartOfToday
						ORDER BY
							DateOfCalc DESC
					)

			--
			-- get Nearby Grocer Status
			--
			DECLARE @NearbyGrocerStatus INT = dbo.fn_NearbyGrocerStatusForSiteFuel(@StartOfToday, @SystemSettings_MaxGrocerDriveTime, @SiteId, @FuelTypeId) 

			SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | CASE @NearbyGrocerStatus
				WHEN 0x00 THEN 0 -- No Nearby Grocers			
				WHEN 0x01 THEN @PriceReasonFlags_HasGrocers | @PriceReasonFlags_HasIncompleteGrocers -- Grocers, but incomplete
				WHEN 0x02 THEN @PriceReasonFlags_HasGrocers -- Grocers and Data
				WHEN 0x03 THEN @PriceReasonFlags_HasGrocers -- Grocers and Data
			END

			IF @NearbyGrocerStatus = 0x01 -- incomplete Grocers data ?
			BEGIN
				IF @Today_Price > 0 AND @Cheapest_SuggestedPrice > @Today_Price
				BEGIN
					SET @Cheapest_SuggestedPrice = @Today_Price
					SET @Cheapest_IsTodayPrice = 1
					SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_TodayPriceSnapBack
				END
			END



			-- handle No Suggested Price
			IF @Cheapest_SuggestedPrice = 0 AND @Today_Price > 0
			BEGIN
				SET @Cheapest_SuggestedPrice = @Today_Price
				SET @Cheapest_IsTodayPrice = 1
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_NoSuggestedPrice 
			END

			-- apply Decimal Rounding (if any)
			IF @SystemSettings_DecimalRounding <> -1 AND @Cheapest_SuggestedPrice > 0
			BEGIN
				SET @Cheapest_SuggestedPrice = dbo.fn_ReplaceLastPriceDigit(@Cheapest_SuggestedPrice, @SystemSettings_DecimalRounding)
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_Rounded
			END
			-- check Price Variance
			IF @Today_Price > 0
			BEGIN
				DECLARE @Diff INT = @Cheapest_SuggestedPrice - @Today_Price

				IF ABS(@Diff) <= @SystemSettings_PriceVariance
				BEGIN
					SET @Cheapest_SuggestedPrice = @Today_Price
					SET @Cheapest_IsTodayPrice = 1 
					SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_TodayPriceSnapBack
				END
			END


			-- check Price Stunt Freeze
			IF @PriceStuntFreeze = 1
			BEGIN
				IF @Today_Price > 0 AND @Cheapest_SuggestedPrice > 0
				BEGIN
					IF @Cheapest_SuggestedPrice > @Today_Price
					BEGIN
						SET @Cheapest_SuggestedPrice = @Today_Price
						SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_PriceStuntFreeze | @PriceReasonFlags_TodayPriceSnapBack
					END
				END
			END

		END
	END

	--
	-- Result
	--
		INSERT INTO @Result
		VALUES (
			@Cheapest_SiteId,
			@Cheapest_FuelTypeId,
			@Cheapest_DateOfCalc,
			@Cheapest_DateOfPrice,
			COALESCE(@Cheapest_SuggestedPrice, 0),
			@Cheapest_UploadId,
			COALESCE(@Cheapest_Markup, 0),
			@Cheapest_CompetitorId,
			@Cheapest_IsTrialPrice,
			@Cheapest_IsTodayPrice,
			@Cheapest_PriceReasonFlags
		)
	
	----DEBUG:START
	--select * from dbo.Site where Id = @SiteId
	--SELECT * FROM @Result
	--SELECT * FROM dbo.Site where Id = @Cheapest_CompetitorId
	----DEBUG:END

	RETURN 
END