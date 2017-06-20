CREATE PROCEDURE [dbo].[spNearbyGrocerPriceStatusForSites]
	@ForDate DATE,
	@DriveTime INT,
	@SiteIds VARCHAR(MAX)
AS
	SET NOCOUNT ON;
		
----DEBUG:START
--DECLARE	@ForDate DATE = '2017-06-20'
--DECLARE	@DriveTime INT = 5
--DECLARE	@SiteIds VARCHAR(MAX) = '6188,9'
----DEBUG:END

	-- constants
	DECLARE @FuelType_SUPER_UNLEADED INT  = 1
	DECLARE @FuelType_UNLEADED INT  = 2
	DECLARE @FuelType_DIESEL INT  = 6

	--
	-- Note: only consider Unleaded and Diesel (Super-Unleaded = Unleaded)
	--
	;WITH UnleadedAndDiesel AS (
		SELECT
			sites.Id [SiteId],
			dbo.fn_NearbyGrocerStatusForSiteFuel(@ForDate, @DriveTime, sites.Id, @FuelType_UNLEADED) [UnleadedNearbyGrocerStatus],
			dbo.fn_NearbyGrocerStatusForSiteFuel(@ForDate, @DriveTime, sites.Id, @FuelType_DIESEL) [DieselNearbyGrocerStatus]
		FROM
			dbo.tf_SplitIdsOnComma(@SiteIds) sites
	)
	SELECT
		uad.SiteId [SiteId],
		uad.UnleadedNearbyGrocerStatus [Unleaded],
		uad.DieselNearbyGrocerStatus [Diesel],
		uad.UnleadedNearbyGrocerStatus [SuperUnleaded]
	FROM 
		UnleadedAndDiesel uad
--RETURN 0