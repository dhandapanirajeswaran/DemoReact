CREATE FUNCTION [dbo].[fn_NearbyGrocerStatusForSiteFuel]
(
	@ForDate DATE,
	@DriveTime INT,
	@SiteId INT,
	@FuelTypeId INT
)
RETURNS TINYINT
AS
BEGIN
	DECLARE @NearbyGrocerStatus TINYINT

----DEBUG:START
--SET NOCOUNT ON;
--DECLARE	@ForDate DATE = GETDATE()
--DECLARE	@DriveTime INT = 5
--DECLARE	@SiteId INT = 1439
--DECLARE	@FuelTypeId INT = 2
----DEBUG:END

	-- constants
	DECLARE @HasNearbyGrocer_Flag TINYINT = 0x01; -- site has 1 or more nearby Grocers
	DECLARE @AllGrocersHavePriceData_Flag TINYINT = 0x02;

	DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @ForDate)

	DECLARE @NearbyGrocersCount INT
	DECLARE @NearbyCompetitorPriceCount INT

	;WITH NearbyCompetitors AS (
		SELECT
			stc.CompetitorId,
			compsite.IsSainsburysSite
			-- debug
			,compsite.SiteName
		FROM
			dbo.SiteToCompetitor stc
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
		WHERE
			stc.SiteId = @SiteId
			AND
			stc.IsExcluded = 0 -- Site Competitor is Not excluded
			AND
			compsite.IsActive = 1 -- Competitor is Active
			AND
			compsite.IsGrocer = 1 -- Competitor is Grocer
			AND
			compsite.IsExcludedBrand = 0 -- Brand is not excluded
			AND
			stc.DriveTime < @DriveTime -- within the X min drive time
	)
	--
	-- Get a list of NON-Sainsburys competitor prices
	--
	,NonSainsburysNearbyCompetitorPrices AS (
		SELECT
			nbc.CompetitorId,
			cp.ModalPrice [CompetitorPrice],
			nbc.IsSainsburysSite [IsSainsburysSite]
			-- debug
			,nbc.SiteName
		FROM
			NearbyCompetitors nbc
			INNER JOIN dbo.CompetitorPrice cp ON cp.SiteId = nbc.CompetitorId AND cp.FuelTypeId = @FuelTypeId AND cp.DateOfPrice = @StartOfYesterday
		WHERE
			nbc.IsSainsburysSite = 0
	),
	--
	-- Get a list of Sainsburys (self competitor) prices
	--
	SainsburysNearbyCompetitorPrices AS (
		SELECT
			nbc.CompetitorId,
			sp.SuggestedPrice,
			nbc.IsSainsburysSite [IsSainsburysSite]
			-- debug
			,nbc.SiteName

		FROM
			NearbyCompetitors nbc
			INNER JOIN dbo.SitePrice sp ON sp.SiteId = nbc.CompetitorId AND FuelTypeId = @FuelTypeId AND sp.DateOfCalc = @StartOfYesterday
		WHERE
			nbc.IsSainsburysSite = 1
	)
	SELECT
		@NearbyCompetitorPriceCount = (SELECT COUNT(1) FROM NonSainsburysNearbyCompetitorPrices) + (SELECT COUNT(1) FROM SainsburysNearbyCompetitorPrices),
		@NearbyGrocersCount = (SELECT COUNT(1) FROM NearbyCompetitors)

	IF @NearbyGrocersCount > 0
	BEGIN
		SET @NearbyGrocerStatus = @HasNearbyGrocer_Flag
		IF @NearbyCompetitorPriceCount = @NearbyGrocersCount
			SET @NearbyGrocerStatus = @HasNearbyGrocer_Flag + @AllGrocersHavePriceData_Flag
	END

	-- result
	RETURN COALESCE(@NearbyGrocerStatus, 0)

	----DEBUG:START
	--SELECT @NearbyGrocerStatus [@NearbyGrocerStatus],
	--	@NearbyGrocersCount [@NearbyGrocersCount],
	--	@NearbyCompetitorPriceCount [@NearbyCompetitorPriceCount]
	----DEBUG:END
END
