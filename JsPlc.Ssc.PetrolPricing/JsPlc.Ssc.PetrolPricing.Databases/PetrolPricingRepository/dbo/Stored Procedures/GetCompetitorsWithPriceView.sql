CREATE PROCEDURE [dbo].[GetCompetitorsWithPriceView]
	@ForDate DATE,
	@SiteId INT
AS
BEGIN
SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = GETDATE()
--DECLARE	@SiteId INT = 2873
----DEBUG:END
	
	-- constants
	DECLARE @MaxDriveTime INT = 25

	DECLARE @ImportProcessStatus_Success INT = 10

	DECLARE @FileUploadTypes_DailyPriceData INT = 1;
	DECLARE @FileUploadTypes_QuarterlySiteData INT = 2;
	DECLARE @FileUploadTypes_LatestJsPriceData INT = 3;
	DECLARE @FileUploadTypes_LatestCompPriceData INT = 4;

	DECLARE @forDateNextDay DATE = DATEADD(DAY, 1, @forDate)
	DECLARE @yesterday DATE = DATEADD(DAY, -1, @forDate)
	DECLARE @yesterdayNextDay DATE = DATEADD(DAY, 1, @yesterday)

	-- resultset #1
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
		CASE 
			WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.Grocers WHERE BrandName = compsite.Brand) THEN 1 
			ELSE 0 
		END [IsGrocer]
	FROM
		dbo.SiteToCompetitor stc
		INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
	WHERE
		stc.SiteId = @SiteId
		AND
		stc.IsExcluded = 0
		AND
		stc.DriveTime < @MaxDriveTime
		AND
		NOT EXISTS(SELECT NULL FROM dbo.ExcludeBrands WHERE BrandName = compsite.Brand)

	;WITH FuelTypes AS (
		SELECT
			*
		FROM 
			dbo.FuelType ft
		WHERE
			ft.Id IN (2, 6, 1) -- 2=Unleaded, 6=Diesel, 1=Super Unleaded
	)
	,JsSite AS (
		SELECT TOP 1
			*
		FROM
			dbo.Site st
		WHERE
			st.Id = @SiteId
	)
	,FileUploadsForDateRange AS (
		SELECT
			*
		FROM
			dbo.FileUpload fu
		WHERE
			fu.StatusId = @ImportProcessStatus_Success
			AND
				fu.UploadDateTime >= @yesterday AND fu.UploadDateTime < @forDateNextDay
	)
	, FileUploadsForToday AS (
		SELECT
			*
		FROM
			FileUploadsForDateRange fufdr
		WHERE
			fufdr.UploadDateTime >= @forDate AND fufdr.UploadDateTime < @forDateNextDay

	), FileUploadsforYesterday AS (
		SELECT
			*
		FROM
			FileUploadsForDateRange fufdr	
		WHERE
			fufdr.UploadDateTime >= @yesterday AND fufdr.UploadDateTime < @yesterdayNextDay
	)
	,CompetitorsWithinDriveTime AS (
		SELECT 
			*
		FROM
			dbo.SiteToCompetitor stc
		WHERE
			stc.SiteId = @SiteId
			AND
			stc.IsExcluded = 0
			AND
			stc.DriveTime < @MaxDriveTime
	),
	fileUploadLatestCompPriceDataToday AS (
		SELECT TOP 1
			*
		FROM
			FileUploadsForToday fu
		WHERE
			fu.id = (SELECT MAX(Id) FROM FileUploadsForToday WHERE UploadTypeId = @FileUploadTypes_LatestCompPriceData)
	)
	,fileUploadLatestCompPricesToday AS (
		SELECT
			*
		FROM
			dbo.LatestCompPrice lcp
		WHERE
			lcp.UploadId = (SELECT Id FROM fileUploadLatestCompPriceDataToday)
	)
	, latestCompPricesToday AS (
		SELECT
			*
		FROM
			dbo.LatestCompPrice lcp
		WHERE
			lcp.UploadId = (SELECT MAX(UploadId) FROM fileUploadLatestCompPricesToday)
	)
	, fileUploadDailyPriceDataToday AS (
		SELECT TOP 1
			*
		FROM
			FileUploadsForToday fuft
		WHERE
			fuft.id = (SELECT MAX(Id) FROM FileUploadsForToday WHERE UploadTypeId = @FileUploadTypes_DailyPriceData)
	)
	, dailyPriceListToday AS (
		SELECT
			*
		FROM
			dbo.DailyPrice dp
		WHERE
			dp.DailyUploadId = (SELECT TOP 1 Id FROM fileUploadDailyPriceDataToday)
	)
	,fileUploadLatestCompPriceDataYesterday AS (
		SELECT TOP 1
			*
		FROM
			FileUploadsforYesterday fufy
		WHERE
			fufy.id = (SELECT MAX(Id) FROM FileUploadsforYesterday WHERE UploadTypeId = @FileUploadTypes_LatestCompPriceData)
	)
	, latestCompPricesYesterday AS (
		SELECT
			*
		FROM
			dbo.LatestCompPrice lcp
		WHERE
			lcp.UploadId = (SELECT MAX(Id) FROM fileUploadLatestCompPriceDataYesterday)
	)
	, fileUploadDailyPriceDataYesterday AS (
		SELECT TOP 1
			*
		FROM
			FileUploadsforYesterday fufy
		WHERE
			fufy.Id = (SELECT MAX(Id) FROM FileUploadsforYesterday WHERE UploadTypeId = @FileUploadTypes_DailyPriceData)
	)
	, dailyPriceListYesterday AS (
		SELECT
			*
		FROM
			dbo.DailyPrice dp
		WHERE
			DailyUploadId = (SELECT TOP 1 Id FROM fileUploadDailyPriceDataYesterday)
	)
	,CompetitorAndFuelsTypes AS (
		SELECT
			comp.CompetitorId [SiteId],
			comp.SiteId [JsSiteId],
			compsite.CatNo [CatNo],
			compsite.SiteName [StoreName],
			compsite.Brand [Brand],
			compsite.Address [Address],
			comp.DriveTime [DriveTime],
			comp.Distance [Distance],
			compsite.Notes [Notes],

			ft.id [FuelTypeId],
			ft.FuelTypeName,

			CASE WHEN jss.TrailPriceCompetitorId = comp.CompetitorId 
				THEN Jss.CompetitorPriceOffsetNew
				ELSE 0 
			END [nOffset],

			COALESCE((SELECT TOP 1 ModalPrice FROM dailyPriceListToday WHERE FuelTypeId = ft.Id AND CatNo = compsite.CatNo), 0) [DailyPriceToday],
			COALESCE((SELECT TOP 1 ModalPrice FROM dailyPriceListYesterday WHERE FuelTypeId = ft.Id AND CatNo = compsite.CatNo), 0) [DailyPriceYesterday],
			COALESCE((SELECT TOP 1 ModalPrice FROM latestCompPricesToday WHERE FuelTypeId = ft.Id AND CatNo = compsite.CatNo), 0) [LatestPriceToday],
			--COALESCE((SELECT TOP 1 ModalPrice FROM test_latestCompPricesYesterday WHERE FuelTypeId = ft.Id AND CatNo = compsite.CatNo), 0) [LatestPriceYesterday]

			COALESCE((
				SELECT TOP 1
					CASE WHEN dbo.SitePrice.OverriddenPrice > 0
						THEN dbo.SitePrice.OverriddenPrice
						ELSE dbo.SitePrice.SuggestedPrice
					END
				FROM 
					dbo.SitePrice
				WHERE
					dbo.SitePrice.Id = (
						SELECT MAX(dbo.SitePrice.Id) 
						FROM dbo.SitePrice 
						INNER JOIN dbo.FileUpload ON dbo.FileUpload.Id = dbo.SitePrice.UploadId
						WHERE dbo.FileUpload.UploadDateTime < @ForDate
						AND dbo.SitePrice.CompetitorId = compsite.Id
						AND dbo.SitePrice.FuelTypeId = ft.Id
						AND (dbo.SitePrice.SuggestedPrice > 0 OR dbo.SitePrice.OverriddenPrice > 0)
					)
			), 0) [LatestPriceYesterday]
		FROM 
			CompetitorsWithinDriveTime comp
			LEFT JOIN dbo.Site compsite ON compsite.Id = comp.CompetitorId AND compsite.IsActive = 1
			CROSS APPLY FuelTypes ft
			CROSS APPLY JsSite jss
		WHERE
			NOT EXISTS(SELECT NULL FROM dbo.ExcludeBrands WHERE BrandName = compsite.Brand)
	)
	, CompetitorsFuelsAndPrices AS (
		SELECT
			caft.SiteId,
			caft.JsSiteId,
			caft.CatNo,
			caft.StoreName,
			caft.Brand,
			caft.Address,
			caft.DriveTime,
			caft.Distance,
			caft.Notes,	
			caft.FuelTypeName,

			caft.FuelTypeId,

			CASE WHEN caft.LatestPriceToday > 0 
				THEN caft.LatestPriceToday + caft.nOffset 
				ELSE CASE WHEN caft.DailyPriceToday > 0 
					THEN caft.DailyPriceToday  + caft.nOffset
					ELSE 0
				END
			END [TodayPrice],

			CASE WHEN caft.LatestPriceYesterday > 0
				--THEN (caft.LatestPriceYesterday / 10) + caft.nOffset
				-- possible fix for issue 4: In site pricing page earlier today we seen some odd site price can we check this also please
				THEN (caft.LatestPriceYesterday) + caft.nOffset
				ELSE CASE 
					WHEN caft.DailyPriceYesterday > 0 THEN caft.DailyPriceYesterday + caft.nOffset
					ELSE 0
					END
			END [YesterdayPrice]

			----- DEBUG
			--, LatestPriceToday [DEBUG_LatestPriceToday]
			--, caft.DailyPriceYesterday [DEBUG_caft.DailyPriceYesterday]
			--, caft.nOffset [DEBUG_caft.nOffset]

		FROM 
			CompetitorAndFuelsTypes caft
	)
	-- resultset #2
	SELECT
		cfp.SiteId,
		cfp.JsSiteId,
		cfp.FuelTypeId,
		cfp.TodayPrice [TodayPrice],
		cfp.YesterdayPrice [YestPrice],
		CASE WHEN cfp.TodayPrice > 0 AND cfp.YesterdayPrice > 0
			THEN cfp.TodayPrice - cfp.YesterdayPrice
			ELSE 0
		END [Difference]

		---- DEBUG
		--,'--DEBUG--'
		--,cfp.[DEBUG_caft.DailyPriceYesterday]
		--,cfp.[DEBUG_caft.nOffset]
		--,cfp.DEBUG_LatestPriceToday
	FROM 
		CompetitorsFuelsAndPrices cfp
END
--RETURN 0