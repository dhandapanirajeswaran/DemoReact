CREATE PROCEDURE [dbo].[spFixZeroSuggestedSitePricesForDay]
	@ForDate DATE
AS
	SET NOCOUNT ON;

	--
	-- Fix SitePrices where SuggestedPrice = 0 but there are previous records
	--
	;With ZeroSuggestedPrices AS (
		SELECT 
			zero.*,
			-- find last good SuggestedPrice for Site Fuel
			(SELECT TOP 1 Id FROM dbo.SitePrice 
			WHERE SiteId = zero.SiteId AND FuelTypeId = zero.FuelTypeId AND SuggestedPrice > 0 AND DateOfCalc <= zero.DateOfCalc 
			ORDER BY DateOfCalc DESC) [LastSuggestedPriceId]
		FROM
			dbo.SitePrice zero
		WHERE
			zero.SuggestedPrice = 0
			AND
			zero.DateOfCalc = @ForDate
	)
	MERGE
		dbo.SitePrice AS target
		USING (
			SELECT
				zsp.Id [ZeroSitePriceId],
				sug.SuggestedPrice [LastSuggestedPrice]
			FROM
				ZeroSuggestedPrices zsp
				INNER JOIN dbo.SitePrice sug ON sug.Id = zsp.LastSuggestedPriceId
		) AS source(ZeroSitePriceId, LastSuggestedPrice)
		ON (source.ZeroSitePriceId = target.Id)
		WHEN MATCHED THEN
			UPDATE SET
			target.SuggestedPrice = source.LastSuggestedPrice;

RETURN 0
