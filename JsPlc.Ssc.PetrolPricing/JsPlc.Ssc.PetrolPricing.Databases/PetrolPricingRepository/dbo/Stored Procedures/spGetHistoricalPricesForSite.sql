CREATE PROCEDURE [dbo].[spGetHistoricalPricesForSite]
	@SiteId INT,
	@StartDate DATE,
	@EndDate DATE
AS
	SET NOCOUNT ON;

	;WITH DateRange As  
	(  
		SELECT @StartDate AS TheDate
		UNION ALL  
		SELECT DATEADD(DAY,1, TheDate) FROM DateRange
		WHERE TheDate < @EndDate  
	)
	SELECT
		dr.TheDate [PriceDate],
		@SiteId [SiteId],
		hfp.FuelTypeId [FuelTypeId],
		COALESCE(hfp.TodayPrice, 0) [TodayPrice],
		COALESCE(hfp.PriceSource, '') [PriceSource]
	FROM
		DateRange dr
		CROSS APPLY dbo.tf_GetHistoricalSiteFuelPricesForDay(dr.TheDate, @SiteId) hfp
	ORDER BY
		dr.TheDate DESC

RETURN 0
