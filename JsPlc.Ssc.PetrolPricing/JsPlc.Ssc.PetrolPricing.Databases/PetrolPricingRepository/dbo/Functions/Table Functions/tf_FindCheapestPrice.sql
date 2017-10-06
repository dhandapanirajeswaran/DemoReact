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
	SiteId INT NOT NULL,
	FuelTypeId INT NOT NULL,
	DateOfCalc DATE NOT NULL,
	DateOfPrice DATE,
	SuggestedPrice INT,
	UploadId INT,
	Markup INT,
	CompetitorId INT,
	IsTrialPrice BIT,
	IsTodayPrice BIT,
	PriceReasonFlags INT,
	DriveTimeMarkup INT,
	CompetitorCount INT,
	CompetitorPriceCount INT,
	GrocerCount INT,
	GrocerPriceCount INT,
	DriveTime REAL,
	NearbyGrocerCount INT,
	NearbyGrocerPriceCount INT
)
AS
BEGIN

----DEBUG:START
--DECLARE	@SiteId INT = 6164
--DECLARE	@FuelTypeId INT = 2
--DECLARE	@ForDate DATE = GETDATE()
--DECLARE	@FileUploadId INT = 8
--DECLARE	@MaxDriveTime INT = 25

--DECLARE @Result TABLE 
--(
--	SiteId INT NOT NULL,
--	FuelTypeId INT NOT NULL,
--	DateOfCalc DATE NOT NULL,
--	DateOfPrice DATE,
--	SuggestedPrice INT,
--	UploadId INT,
--	Markup INT,
--	CompetitorId INT,
--	IsTrialPrice BIT,
--	IsTodayPrice BIT,
--	PriceReasonFlags INT,
--	DriveTimeMarkup INT,
--	CompetitorCount INT,
--	CompetitorPriceCount INT,
--	GrocerCount INT,
--	GrocerPriceCount INT,
--	DriveTime REAL,
--	NearbyGrocerCount INT,
--	NearbyGrocerPriceCount INT
--)
----DEBUG:END

	DECLARE @PriceStuntFreeze BIT = dbo.fn_IsPriceFreezeActiveForDate(@ForDate, @FuelTypeId);

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
	DECLARE @PriceReasonFlags_ManualOverride INT = 0x00004000
	DECLARE @PriceReasonFlags_MatchCompetitorFound INT = 0x00008000

	DECLARE @PriceReasonFlags_TrialPriceFound INT = 0x00010000

	-- Price Match Type
	DECLARE @PriceMatchType_Standard INT = 1
	DECLARE @PriceMatchType_TrialPrice INT = 2
	DECLARE @PriceMatchType_MatchCompetitor INT = 3
	--
	-- Get SystemSettings
	--
	DECLARE @SystemSettings_NearbyGrocerDriveTime INT
	DECLARE @SystemSettings_PriceVariance INT
	DECLARE @SystemSettings_DecimalRounding INT

	SELECT TOP 1 
		@SystemSettings_NearbyGrocerDriveTime = MaxGrocerDriveTimeMinutes,
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
	DECLARE @Cheapest_DriveTimeMarkup INT = 0
	DECLARE @Cheapest_CompetitorCount INT = 0
	DECLARE @Cheapest_CompetitorPriceCount INT = 0
	DECLARE @Cheapest_GrocerCount INT = 0
	DECLARE @Cheapest_GrocerPriceCount INT = 0
	DECLARE @Cheapest_DriveTime REAL = 0
	DECLARE @Cheapest_NearbyGrocerCount INT = 0
	DECLARE @Cheapest_NearbyGrocerPriceCount INT = 0

	DECLARE @MatchCompetitorPriceFound INT = NULL
	DECLARE @MinPriceCompetitorId INT = NULL

	DECLARE @HasNearbyGrocers BIT = 0
	DECLARE @HasIncompleteNearbyGrocerPriceData BIT = 0

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
	DECLARE @Site_MatchCompetitorIsSainsburysSite BIT = 0

	SELECT TOP 1
		@Site_CatNo = COALESCE(st.CatNo, 0),
		@Site_PfsNo = COALESCE(st.PfsNo, 0),
		@Site_StoreNo = COALESCE(st.StoreNo, 0),
		@Site_PriceMatchType = st.PriceMatchType,
		@Site_MatchCompetitorSiteId = CASE WHEN st.PriceMatchType = @PriceMatchType_MatchCompetitor THEN st.TrailPriceCompetitorId ELSE NULL END,
		@Site_MatchCompetitorMarkup = CASE WHEN st.PriceMatchType = @PriceMatchType_MatchCompetitor THEN COALESCE(st.CompetitorPriceOffsetNew, 0) ELSE 0 END,
		@Site_TrialPriceMarkup = CASE WHEN st.PriceMatchType = @PriceMatchType_TrialPrice THEN COALESCE(st.CompetitorPriceOffset, 0) ELSE 0 END
	FROM
		dbo.Site st
	WHERE
		st.Id = @SiteId
	--
	-- Check if Daily Catalist file exists for Date	(NOTE: always for Success, Processing, Calculating)
	--
	DECLARE @IsDailyCatalistFileForDate BIT = CASE 
		WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId IN (5, 10, 11) AND UploadTypeId = 1 AND UploadDateTime BETWEEN @StartOfToday AND @StartOfTomorrow)
		THEN 1
		ELSE 0
	END

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
		-- Get most recent Sainsburys 'Today' price for Site Fuel (if any)
		--
		DECLARE @Today_Price INT = dbo.fn_TodayPriceForSiteFuelDate(@SiteId, @FuelTypeId, @ForDate)

		-- lookup Competitor, Grocer and Nearby Grocer Data % counts
		SELECT
			@Cheapest_CompetitorCount = ncd.CompetitorCount,
			@Cheapest_CompetitorPriceCount = ncd.CompetitorPriceCount,
			@Cheapest_GrocerCount = ncd.GrocerCount,
			@Cheapest_GrocerPriceCount = ncd.GrocerPriceCount,
			@Cheapest_NearbyGrocerCount = ncd.NearbyGrocerCount,
			@Cheapest_NearbyGrocerPriceCount = ncd.NearbyGrocerPriceCount
		FROM
			dbo.tf_NearbyCompetitorDataSummaryForSiteFuel(@ForDate, @MaxDriveTime, @SiteId, @FuelTypeId, @SystemSettings_NearbyGrocerDriveTime) ncd

		-- Determine Nearby Grocer status
		IF @Cheapest_NearbyGrocerCount <> 0
		BEGIN
			SET @HasNearbyGrocers = 1
			SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_HasGrocers

			IF @Cheapest_NearbyGrocerCount = @Cheapest_NearbyGrocerPriceCount
			BEGIN
				SET @HasIncompleteNearbyGrocerPriceData = 0
			END
			ELSE
			BEGIN
				SET @HasIncompleteNearbyGrocerPriceData = 1
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_HasIncompleteGrocers
			END
		END

		--
		-- Price Strategy Match: Match Competitor ?
		--
		IF @Site_PriceMatchType = @PriceMatchType_MatchCompetitor
		BEGIN
			--
			-- Determine if Match Competitor Site is a Sainsburys site
			--
			SET @Site_MatchCompetitorIsSainsburysSite = CASE 
				WHEN (SELECT TOP 1 IsSainsburysSite FROM dbo.Site WHERE Id = @Site_MatchCompetitorSiteId) = 1 THEN 1 
				ELSE 0 
			END

			--
			-- Search for most recent COMPETITOR Site Price for Fuel on Date
			--
			IF @Site_MatchCompetitorIsSainsburysSite = 1
			BEGIN
				SELECT TOP 1
					@MatchCompetitorPriceFound = CASE 
						WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
						WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
						ELSE NULL
					END,
					@MinPriceCompetitorId = sp.SiteId
				FROM
					dbo.SitePrice sp
				WHERE
					sp.SiteId = @Site_MatchCompetitorSiteId
					AND
					sp.FuelTypeId = @FuelTypeId
					AND
					sp.DateOfCalc <= @StartOfYesterday
				ORDER BY
					sp.DateOfCalc DESC
			END
			ELSE
			BEGIN
				-- Non-Sainsburys Match Competitor Site
				SELECT TOP 1
					@MatchCompetitorPriceFound = cp.ModalPrice,
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
							AND DateOfPrice <= @StartOfYesterday
						ORDER BY
							DateOfPrice DESC
						)
			END

			IF @MatchCompetitorPriceFound > 0
			BEGIN
				-- Found price for Match Competitor site
				SET @Cheapest_SuggestedPrice = @MatchCompetitorPriceFound + @Site_MatchCompetitorMarkup * 10
				SET @Cheapest_CompetitorId = @MinPriceCompetitorId
				SET @Cheapest_Markup = @Site_MatchCompetitorMarkup
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_MatchCompetitorFound
			END
			ELSE
			BEGIN
				-- No price found for Match Competitor site
				SET @Cheapest_SuggestedPrice = @Today_Price
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_NoMatchCompetitorPrice
			END
		END

		--
		-- Price Strategy Match: Standard Price OR Trial price ?
		--
		IF (@Site_PriceMatchType = @PriceMatchType_Standard OR @Site_PriceMatchType = @PriceMatchType_TrialPrice)
		BEGIN
			--
			-- Find the cheapest 0-25 min Competitor
			--	
			;WITH NonSainsburysCompetitorPrices as ( -- NON-Sainsburys
				SELECT
					stc.CompetitorId,
					stc.Distance,
					stc.DriveTime,
					cp.ModalPrice,
					dbo.fn_GetDriveTimePence(@FuelTypeId, stc.DriveTime) * 10 [DriveTimeMarkup],
					cp.DailyPriceId,
					cp.LatestCompPriceId,
					NULL [SitePriceId]
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
				SainsburysCompetitorPrices AS (
					SELECT
						stc.CompetitorId,
						stc.Distance,
						stc.DriveTime,
						CASE
							WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
							WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
							ELSE 0
						END [ModalPrice],
						dbo.fn_GetDriveTimePence(@FuelTypeId, stc.DriveTime) * 10 [DriveTimeMarkup],
						NULL [DailyPriceId],
						NULL [LatestCompPriceId],
						sp.Id [SitePriceId]
					FROM
						dbo.SiteToCompetitor stc
						INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
						INNER JOIN dbo.SitePrice sp ON sp.Id = (
							SELECT TOP 1
								Id
							FROM
								dbo.SitePrice
							WHERE
								SiteId = stc.CompetitorId
								AND
								FuelTypeId = @FuelTypeId
								AND
								DateOfCalc <= @StartOfYesterday
							ORDER BY
								DateOfCalc DESC
						) --stc.CompetitorId AND sp.FuelTypeId = @FuelTypeId AND sp.DateOfCalc = @StartOfYesterday
					WHERE
						stc.SiteId = @SiteId
						AND
						stc.IsExcluded = 0 -- Site Competitor is not excluded
						AND 
						compsite.IsActive = 1 -- Competitor site is active
						AND
						compsite.IsExcludedBrand = 0 -- Not an Excluded Brand
						AND
						stc.DriveTime < @MaxDriveTime
				),
				NearbyCompetitorPricesIncDriveTime AS (
					SELECT
						nacp.*,
						nacp.ModalPrice + nacp.DriveTimeMarkup [PriceIncDriveTime]
					FROM
						NonSainsburysCompetitorPrices nacp
					UNION ALL
					SELECT
						nbsp.*,
						nbsp.ModalPrice + nbsp.DriveTimeMarkup [PriceIncDriveTime]
					FROM
						SainsburysCompetitorPrices nbsp
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
						WHEN bcp.SitePriceId IS NOT NULL
						THEN sp.DateOfCalc
						ELSE ''
					END,
					@Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | 
						CASE @Site_PriceMatchType
							WHEN @PriceMatchType_Standard THEN @PriceReasonFlags_CheapestPriceFound
							WHEN @PriceMatchType_TrialPrice THEN @PriceReasonFlags_CheapestPriceFound | @PriceReasonFlags_TrialPriceFound
							ELSE 0
						END
				FROM 
					BestCompetitorPrice bcp
					LEFT JOIN dbo.DailyPrice dp ON dp.Id = bcp.DailyPriceId
					LEFT JOIN dbo.LatestCompPrice lcp ON lcp.Id = bcp.LatestCompPriceId
					LEFT JOIN dbo.SitePrice sp ON sp.Id = bcp.SitePriceId

			--
			-- Incomplete Nearby Grocer data ?
			--
			IF @HasIncompleteNearbyGrocerPriceData = 1 -- incomplete Grocers data ?
			BEGIN
				-- Is Today Price more expensive than the Cheapest Competitor price (inc drive time markup) ?
				IF @Today_Price > 0 AND @Today_Price > @Cheapest_SuggestedPrice
				BEGIN
					SET @Cheapest_SuggestedPrice = @Cheapest_SuggestedPrice; -- use the Cheapest Competitor price
				END
				ELSE
				BEGIN
					IF @Today_Price > 0
					BEGIN
						-- Today Price is cheaper than the Cheapest Competitor price (do NOT move price due to incomplete Grocer data)
						SET @Cheapest_SuggestedPrice = @Today_Price
						SET @Cheapest_IsTodayPrice = 1
						SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_TodayPriceSnapBack
					END
				END
			END -- @HasIncompleteNearbyGrocerPriceData = 1 
		END --  @PriceMatchType_Standard OR @PriceMatchType_TrialPrice)

		--
		-- handle No Suggested Price
		--
		IF @Cheapest_SuggestedPrice = 0 AND @Today_Price > 0
		BEGIN
			SET @Cheapest_SuggestedPrice = @Today_Price
			SET @Cheapest_IsTodayPrice = 1
			SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_NoSuggestedPrice 
		END

		--
		-- apply Decimal Rounding (if any)
		--
		IF @SystemSettings_DecimalRounding <> -1 AND @Cheapest_SuggestedPrice > 0
		BEGIN
			SET @Cheapest_SuggestedPrice = dbo.fn_ReplaceLastPriceDigit(@Cheapest_SuggestedPrice, @SystemSettings_DecimalRounding)
			SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_Rounded
		END

		--
		-- check Price Variance
		--
		IF @Today_Price > 0 AND @Cheapest_SuggestedPrice > 0
		BEGIN
			DECLARE @Diff INT = @Cheapest_SuggestedPrice - @Today_Price

			IF ABS(@Diff) <= @SystemSettings_PriceVariance
			BEGIN
				-- Within Price Variance
				SET @Cheapest_SuggestedPrice = @Today_Price
				SET @Cheapest_IsTodayPrice = 1 
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_TodayPriceSnapBack | @PriceReasonFlags_InsidePriceVariance
			END
			ELSE
			BEGIN
				-- Outside Price Variance
				SET @Cheapest_PriceReasonFlags = @Cheapest_PriceReasonFlags | @PriceReasonFlags_OutsidePriceVariance
			END
		END

		--
		-- check Price Stunt Freeze
		--
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

		-- lookup Drive Time markup
		IF @Cheapest_CompetitorId > 0
		BEGIN
			SELECT TOP 1 
				@Cheapest_DriveTimeMarkup = dbo.fn_GetDriveTimePence(@FuelTypeId, stc.DriveTime),
				@Cheapest_DriveTime = stc.DriveTime
			FROM
				dbo.SiteToCompetitor stc
			WHERE
				stc.SiteId = @SiteId
				AND
				stc.CompetitorId = @Cheapest_CompetitorId
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
			@Cheapest_PriceReasonFlags,
			@Cheapest_DriveTimeMarkup,
			@Cheapest_CompetitorCount,
			@Cheapest_CompetitorPriceCount,
			@Cheapest_GrocerCount,
			@Cheapest_GrocerPriceCount,
			COALESCE(@Cheapest_DriveTime, 0.0),
			@Cheapest_NearbyGrocerCount,
			@Cheapest_NearbyGrocerPriceCount
		)
	
	----DEBUG:START
	--SELECT * FROM @Result
	--SELECT
	--	(SELECT TOP 1 SiteName FROM dbo.Site WHERE Id = @SiteId) [SiteName],
	--	dbo.fn_GetPriceReasons(@Cheapest_PriceReasonFlags) [@Cheapest_PriceReasonFlags], 
	--	@Site_PriceMatchType [@Site_PriceMatchType],
	--	@Cheapest_CompetitorId [@Cheapest_CompetitorId],
	--	@Cheapest_SuggestedPrice [@Cheapest_SuggestedPrice],
	--	@Cheapest_DriveTimeMarkup [@Cheapest_DriveTimeMarkup],
	--	(SELECT TOP 1 SiteName FROM dbo.Site WHERE Id = @Cheapest_CompetitorId) [Cheapest Competitor Site]
	----DEBUG:END

	RETURN 
END