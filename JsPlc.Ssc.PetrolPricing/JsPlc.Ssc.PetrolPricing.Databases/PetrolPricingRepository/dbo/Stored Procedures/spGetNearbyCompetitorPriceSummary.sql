CREATE PROCEDURE [dbo].[spGetNearbyCompetitorPriceSummary]
	@ForDate DATE,
	@SiteIds VARCHAR(MAX),
	@DriveTime INT
AS
BEGIN
	SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-07-20'
--DECLARE	@SiteIds VARCHAR(MAX) = '6164'
--DECLARE	@DriveTime INT = 25
----DEBUG:END

	-- consider only Unleaded and Diesel (Super-Unleaded = Unleaded)
	DECLARE @FuelTypes TABLE (FuelTypeId INT);
	INSERT INTO @FuelTypes
	SELECT 2 -- UNLEADED
	UNION ALL
	SELECT 6 -- DIESEL
	UNION ALL
	SELECT 1 -- SUPER-UNLEADED

	;With MainSiteFuelsSummary AS (
		SELECT
			ids.Id,
			ft.FuelTypeId,
			ncd.CompetitorCount [CompetitorCount],
			ncd.GrocerCount [GrocerCount],
			ncd.CompetitorPriceCount [CompetitorPriceCount],
			ncd.GrocerPriceCount [GrocerPriceCount]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) ids
			CROSS APPLY @FuelTypes ft
			CROSS APPLY dbo.tf_NearbyCompetitorDataSummaryForSiteFuel(@ForDate, @DriveTime, ids.Id, ft.FuelTypeId) ncd
	)
	-- Unleaded and Diesel
	SELECT
		msfs.Id [SiteId],
		msfs.FuelTypeId [FuelTypeId],
		msfs.CompetitorCount,
		msfs.CompetitorPriceCount,
		msfs.GrocerCount,
		msfs.GrocerPriceCount
	FROM 
		MainSiteFuelsSummary msfs
END
--RETURN 0
