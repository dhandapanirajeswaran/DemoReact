﻿CREATE PROCEDURE dbo.spCalculateSitePricesForDate (
	@forDate DATE,
	@SiteIds VARCHAR(MAX)
)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = GetDate()
--DECLARE @SiteIds VARCHAR(MAX) = '9'
----DEBUG:END

	DECLARE @forDateNextDay DATE = DATEADD(DAY, 1, @forDate)

	DECLARE @catalistFileExits bit = 
		CASE WHEN EXISTS(SELECT NULL FROM FileUpload WHERE UploadTypeId = 1 AND UploadDateTime >= @forDate AND UploadDateTime < @forDateNextDay) 
			THEN 1 
			ELSE 0 
		END

	-- DEBUG
	-- SELECT @catalistFileExits [@catalistFileExits]

	DECLARE @FuelsTypesTV TABLE (FuelTypeId INT, FuelMarkup INT, AliasFuelTypeId INT)
	INSERT INTO @FuelsTypesTV
	VALUES 
		(1, 50, 2),	-- Super Unleaded - NOTE: maps to Unleaded with 5.0p markup
		(2, 0, 2),	-- Unleaded
		(6, 0, 6)	-- Diesel

	;WITH SiteFuelsCTE AS (
		SELECT
			ids.Id [SiteId],
			st.CompetitorPriceOffset,
			st.TrailPriceCompetitorId,
			st.PfsNo,
			st.StoreNo,
			st.CatNo,
			st.CompetitorPriceOffset * 10 [TrialPrice],
			ft.FuelTypeId,
			ft.AliasFuelTypeId,
			ft.FuelMarkup
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			INNER JOIN dbo.Site st on st.Id = ids.Id
			CROSS APPLY @FuelsTypesTV ft
	)
	, SitePriceRowCTE AS (
		SELECT
			sf.SiteId,
			sf.FuelTypeId,
			sf.AliasFuelTypeId,
			CASE WHEN spd.Id IS NOT NULL 
				THEN COALESCE(dbo.fn_ReplaceLastPriceDigit(spd.SuggestedPrice + sf.FuelMarkup, '9'), 0)
				ELSE 0
			END [AutoPrice],
			CASE WHEN dpo.Id IS NOT NULL
				THEN dbo.fn_ReplaceLastPriceDigit(dpo.OverriddenPrice + sf.TrialPrice, '9')
				ELSE 0 
			END [overridePrice],
			CASE WHEN spd.Id IS NULL 
				THEN 0
				ELSE spd.Markup 
			END [Markup],
			CASE WHEN comp.Id IS NULL 
				THEN 'Unknown' 
				ELSE comp.Brand + '/' + comp.SiteName 
			END [CompetitorName],
			CASE WHEN spd.Id IS NULL
				THEN 0
				ELSE spd.IsTrailPrice
			END [IsTrailPrice],
			sf.CompetitorPriceOffset [CompetitorPriceOffset],
			sf.PfsNo [PfsNo],
			sf.StoreNo [StoreNo],
			sf.CatNo [CatNo],
			sf.CompetitorPriceOffset * 10 [TrialPrice],
			sf.FuelMarkup
		FROM
			SiteFuelsCTE sf

			-- sitePriceData
			LEFT JOIN dbo.SitePrice spd on spd.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sf.SiteId AND (SuggestedPrice > 0 OR OverriddenPrice > 0))

			-- dieselPriceOverride
			LEFT JOIN dbo.SitePrice dpo on spd.Id IS NOT NULL AND dpo.Id = (SELECT MAX(id) FROM dbo.SitePrice WHERE SiteId = sf.SiteId AND FuelTypeId = sf.AliasFuelTypeId AND DateOfPrice >= @forDate AND DateOfPrice < @forDateNextDay AND OverriddenPrice > 0)

			-- competitor site
			LEFT JOIN dbo.Site comp on comp.Id = sf.TrailPriceCompetitorId 
	)
	, TodayPriceCTE AS (
		SELECT 
			spr.SiteId,
			spr.FuelTypeId,
			spr.AutoPrice,
			spr.overridePrice,
			dbo.fn_ReplaceLastPriceDigit(
				dbo.fn_CalculateTodayPrice(
					fu.UploadDateTime,
					lp.Id,
					lp.ModalPrice,
					ovp.Id,
					ovp.DateOfPrice,
					ovp.OverriddenPrice,
					tp.Id,
					tp.DateOfPrice,
					tp.ModalPrice
				) + spr.FuelMarkup, '9') [TodayPrice],
			spr.Markup,
			spr.CompetitorName,
			spr.IsTrailPrice,
			spr.CompetitorPriceOffset
		FROM 
			SitePriceRowCTE spr

			-- LatestPrice
			LEFT JOIN dbo.LatestPrice lp ON lp.Id = (SELECT MIN(Id) FROM dbo.LatestPrice WHERE PfsNo = spr.PfsNo AND StoreNo = spr.StoreNo AND FuelTypeId = spr.FuelTypeId)

			-- OverridePriceIfAny
			LEFT JOIN dbo.SitePrice ovp ON ovp.Id = (SELECT MAX(Id) FROM dbo.SitePrice WHERE SiteId = spr.SiteId AND FuelTypeId = spr.FuelTypeId AND OverriddenPrice > 0)

			-- TodayPriceSortByDate
			LEFT JOIN dbo.DailyPrice tp ON tp.id = (SELECT MAX(Id) FROM dbo.DailyPrice WHERE CatNo = spr.CatNo AND FuelTypeId = spr.FuelTypeId AND ModalPrice > 0)

			-- FileUpload
			LEFT JOIN dbo.FileUpload fu ON fu.Id = lp.UploadId
	)
	-- Resultset
	SELECT 
		tp.SiteId [SiteId],
		tp.FuelTypeId [FuelTypeId],
		CASE WHEN @catalistFileExits = 1
			THEN tp.AutoPrice
			ELSE 0
		END [AutoPrice],
		tp.overridePrice [OverridePrice],
		tp.TodayPrice [TodayPrice],
		tp.Markup [Markup],
		tp.CompetitorName [CompetitorName],
		tp.IsTrailPrice [IsTrailPrice],
		tp.CompetitorPriceOffset [CompetitorPriceOffset]
	FROM 
		TodayPriceCTE tp

END