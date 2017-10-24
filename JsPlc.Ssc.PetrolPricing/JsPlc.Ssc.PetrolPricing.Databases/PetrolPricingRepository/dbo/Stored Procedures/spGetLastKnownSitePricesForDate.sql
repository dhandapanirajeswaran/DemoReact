CREATE PROCEDURE [dbo].[spGetLastKnownSitePricesForDate]
	@DateFrom DATE,
	@SiteIds VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DayBefore DATE = DATEADD(DAY, -1, @DateFrom)

	;WITH SiteFuelPriceIds AS (
		SELECT
			ids.Id [SiteId],
			ft.Id [FuelTypeId],
			(SELECT MAX(Id) FROM dbo.SitePrice WHERE SiteId = ids.Id AND FuelTypeId = ft.Id AND DateOfCalc < @DayBefore AND (SuggestedPrice > 0 OR OverriddenPrice > 0) ) [SitePriceId]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			CROSS APPLY dbo.FuelType ft WHERE ft.Id IN (1, 2, 6)
	)
	SELECT
		sfpi.SiteId [SiteId],
		sfpi.FuelTypeId [FuelTypeId],
		CASE 
			WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
			WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
			ELSE 0
		END [Price],
		sp.DateOfCalc [DateOfCalc],
		sp.Id [SitePriceId]
	FROM 
		SiteFuelPriceIds sfpi
		LEFT JOIN dbo.SitePrice sp ON sp.Id = sfpi.SitePriceId
END

