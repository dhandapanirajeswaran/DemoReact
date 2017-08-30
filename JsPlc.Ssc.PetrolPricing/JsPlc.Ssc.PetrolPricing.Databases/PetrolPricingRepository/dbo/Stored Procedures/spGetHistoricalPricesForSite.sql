CREATE PROCEDURE [dbo].[spGetHistoricalPricesForSite]
	@SiteId INT,
	@StartDate DATE,
	@EndDate DATE
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE	@SiteId INT = 6164
--DECLARE	@StartDate DATE = DATEADD(DAY, -30, GETDATE())
--DECLARE @EndDate DATE = GETDATE()
----DEBUG:END

	;WITH DateRange As  
	(  
		SELECT @StartDate AS TheDate
		UNION ALL  
		SELECT DATEADD(DAY,1, TheDate) FROM DateRange
		WHERE TheDate < @EndDate  
	),
	FuelTypes AS (
		SELECT 1 [FuelTypeId]
		UNION ALL
		SELECT 2 [FuelTypeId]
		UNION ALL
		SELECT 6 [FuelTypeId]
	),
	HistoricalPrices AS (
		SELECT
			dr.TheDate [PriceDate],
			@SiteId [SiteId],
			hfp.FuelTypeId [FuelTypeId],
			hfp.TodayPrice [TodayPrice],
			PriceSource [PriceSource],
			hfp.PriceReasonFlags [PriceReasonFlags]
		FROM
			DateRange dr
			CROSS APPLY dbo.tf_GetHistoricalSiteFuelPricesForDay(dr.TheDate, @SiteId) hfp
	)
	SELECT
		DATEADD(DAY, 1, dr.TheDate) [PriceDate],
		@SiteId [SiteId],
		ft.FuelTypeId [FuelTypeId],
		COALESCE(hp.TodayPrice, 0) [TodayPrice],
		COALESCE(hp.PriceSource, '') [PriceSource],
		COALESCE(hp.PriceReasonFlags, 0) [PriceReasonFlags]
	FROM
		DateRange dr
		CROSS APPLY FuelTypes ft
		LEFT JOIN HistoricalPrices hp ON hp.FuelTypeId = ft.FuelTypeId AND hp.PriceDate = dr.TheDate
	ORDER BY
		dr.TheDate DESC,
		CASE ft.FuelTypeId 
			WHEN 2 THEN 1	-- Unleaded (1st)
			WHEN 6 THEN 2	-- Diesel (2nd)
			WHEN 1 THEN 3	-- Super-Unleaded (3rd)
		END ASC

	RETURN
END