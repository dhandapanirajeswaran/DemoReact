CREATE PROCEDURE [dbo].[spSetSitePriceMatchTypeDefaults]
AS
	SET NOCOUNT ON

	UPDATE 
		st
	SET 
		PriceMatchType = CASE 
			WHEN st.TrailPriceCompetitorId IS NOT NULL THEN 3 -- Match Competitor
			WHEN st.CompetitorPriceOffset <> 0 THEN 2 -- Trail Price
			ELSE 1 -- Solo Price
		END
	FROM 
		dbo.Site st
	WHERE
		st.PriceMatchType IS NULL
		OR
		st.PriceMatchType = 0
RETURN 0
