﻿CREATE FUNCTION [dbo].[tf_GetHistoricalSiteFuelPricesForDay]
(
	@ForDate DATE,
	@SiteIds VARCHAR(MAX)
)
RETURNS 
@ResultTV TABLE 
(
	SiteId INT,
	FuelTypeId INT,
	TodayPrice INT,
	PriceSource VARCHAR(20),
	PriceReasonFlags INT
)
AS
BEGIN

	-- Constants
	DECLARE @PriceReasonFlags_ManualOverride INT = 0x00004000

	;WITH SiteFuelCombos AS (
		SELECT
			ids.Id [SiteId],
			ft.Id [FuelTypeId]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1,2,6)) ft
	)
	INSERT INTO @ResultTV
	SELECT
		sfc.SiteId,
		sfc.FuelTypeId,
		CASE
			WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
			WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
			ELSE 0
		END [TodayPrice],
		CASE	
			WHEN sp.OverriddenPrice > 0 THEN 'Override'
			WHEN sp.SuggestedPrice > 0 THEN 'Suggested'
			ELSE ''
		END [PriceSource],
		CASE
			WHEN sp.OverriddenPrice > 0 THEN @PriceReasonFlags_ManualOverride
			ELSE sp.PriceReasonFlags
		END [PriceReasonFlags]
	FROM
		SiteFuelCombos sfc
		INNER JOIN dbo.Site st ON st.Id = sfc.SiteId
		LEFT JOIN dbo.SitePrice sp ON sp.SiteId = sfc.SiteId AND sp.FuelTypeId = sfc.FuelTypeId AND DateOfCalc = @ForDate
	WHERE
		st.IsSainsburysSite = 1

	UNION ALL
	SELECT
		sfc.SiteId,
		sfc.FuelTypeId,
		cp.ModalPrice [TodayPrice],
		CASE	
			WHEN cp.ModalPrice > 0 THEN 'Competitor'
			ELSE ''
		END [PriceSource],
		0 [PriceReasonFlags]
	FROM
		SiteFuelCombos sfc
		INNER JOIN dbo.Site st ON st.Id = sfc.SiteId
		LEFT JOIN dbo.CompetitorPrice cp ON cp.SiteId = sfc.SiteId AND cp.FuelTypeId = sfc.FuelTypeId AND cp.DateOfPrice = @ForDate
	WHERE
		st.IsSainsburysSite = 0
	RETURN 
END