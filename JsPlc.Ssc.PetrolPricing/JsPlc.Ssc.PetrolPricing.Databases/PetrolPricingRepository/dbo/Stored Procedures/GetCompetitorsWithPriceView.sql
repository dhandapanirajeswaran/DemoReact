CREATE PROCEDURE [dbo].[GetCompetitorsWithPriceView]
	@ForDate DATE,
	@SiteId INT
AS
BEGIN
	SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = GetDate()
--DECLARE	@SiteId INT = 8
----DEBUG:END
	
	-- constants
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
		compsite.Notes [Notes]
	FROM
		dbo.SiteToCompetitor stc
		INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId AND compsite.IsActive = 1
	WHERE
		stc.SiteId = @SiteId
		AND
		stc.IsExcluded = 0
		AND
		stc.DriveTime < 25
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
			fu.StatusId = 10
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
			stc.DriveTime < 25
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
			lcp.UploadId = (SELECT MAX(Id) FROM fileUploadLatestCompPricesToday)
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
			COALESCE((SELECT TOP 1 ModalPrice FROM latestCompPricesYesterday WHERE FuelTypeId = ft.Id AND CatNo = compsite.CatNo), 0) [LatestPriceYesterday]
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
				ELSE caft.DailyPriceToday + caft.nOffset
			END [TodayPrice],

			CASE WHEN caft.LatestPriceYesterday > 0
				THEN (caft.LatestPriceYesterday / 10) + caft.nOffset
				ELSE caft.DailyPriceYesterday + caft.nOffset
			END [YesterdayPrice]
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
	FROM 
		CompetitorsFuelsAndPrices cfp

END
RETURN 0