CREATE FUNCTION [dbo].[tf_NearbyCompetitorDataSummaryForSiteFuel]
(
	@ForDate DATE,
	@CompetitorDriveTime INT,
	@SiteId INT,
	@FuelTypeId INT,
	@NearbyGrocerDriveTime INT
)
RETURNS @results TABLE
(
	CompetitorCount INT,
	GrocerCount INT,
	CompetitorPriceCount INT,
	GrocerPriceCount INT,
	NearbyGrocerCount INT,
	NearbyGrocerPriceCount INT
)
AS
BEGIN

----DEBUG:START
--DECLARE	@ForDate DATE = GETDATE()
--DECLARE	@CompetitorDriveTime INT = 25
--DECLARE	@SiteId INT = 6164
--DECLARE	@FuelTypeId INT = 2
--DECLARE @NearbyGrocerDriveTime INT = 5

--DECLARE @results TABLE
--(
--	CompetitorCount INT,
--	GrocerCount INT,
--	CompetitorPriceCount INT,
--	GrocerPriceCount INT,
--	NearbyGrocerCount INT,
--	NearbyGrocerPriceCount INT
--)
----DEBUG:END


	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate);
	DECLARE @yesterday DATE = DATEADD(DAY, -1, @forDate)
	DECLARE @yesterdayNextDay DATE = DATEADD(DAY, 1, @yesterday)

	-- constants
	DECLARE @ImportProcessStatus_Success INT = 10

	-- results
	DECLARE @CompetitorCount INT;
	DECLARE @GrocerCount INT;
	DECLARE @CompetitorPriceCount INT = 0;
	DECLARE @GrocerPriceCount INT = 0;
	DECLARE @NearbyGrocerCount INT = 0;
	DECLARE @NearbyGrocerPriceCount INT = 0;

	DECLARE @LastCompetitorDateOfPrice DATE = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE DateOfPrice <= @ForDate)

	DECLARE @LastSitePriceDateOfCalc DATE = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE DateOfCalc <= DATEADD(DAY, -1, @ForDate))

	--
	-- find Competitors within (e.g. 25) minute drive time
	--
	-- conditions:
	--		(1) within X minutes drive time
	--		(2) Brand is NOT excluded
	--		(3) Competitor site is Active
	--		(4) Grocer OR non-Grocers
	--		(5) Nearby Grocers (e.g. within 5 min drive time)
	--
	DECLARE @NearbyCompetitors TABLE (CompetitorSiteId INT PRIMARY KEY, CatNo INT, IsGrocer BIT, IsSainsburysSite BIT, IsNearbyGrocer BIT)
	INSERT INTO @NearbyCompetitors
		SELECT
			stc.CompetitorId [CompetitorSiteId],
			compsite.CatNo [CatNo],
			compsite.isGrocer [IsGrocer],
			compsite.IsSainsburysSite [IsSainsburysSite],
			CASE 
				WHEN compsite.IsGrocer = 1 AND stc.DriveTime <= @NearbyGrocerDriveTime THEN 1
				ELSE 0
			END [IsNearbyGrocer]
		FROM
			dbo.SiteToCompetitor stc
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
		WHERE
			stc.SiteId = @SiteId
			AND
			stc.DriveTime < @CompetitorDriveTime -- within Competitor drive time
			AND
			compsite.IsActive = 1 -- Active competitor
			AND
			compsite.IsExcludedBrand = 0 -- ignore Excluded brands
			AND
			stc.IsExcluded = 0 -- ignore Excluded Site Competitors

	--
	-- get nearby Competitor and Grocer Counts
	--
	SELECT
		@CompetitorCount = (SELECT COUNT(1) FROM @NearbyCompetitors),
		@GrocerCount = (SELECT COUNT(1) FROM @NearbyCompetitors WHERE IsGrocer=1),
		@NearbyGrocerCount = (SELECT COUNT(1) FROM @NearbyCompetitors WHERE IsNearbyGrocer = 1)

	--
	-- get nearby Competitor and Grocer price data (if any)
	--
	;WITH NearbyPriceData AS (
		SELECT
			nbc.IsGrocer,
			nbc.IsNearbyGrocer
		FROM
			@NearbyCompetitors nbc
			INNER JOIN dbo.CompetitorPrice cp ON cp.SiteId = nbc.CompetitorSiteId AND cp.FuelTypeId = @FuelTypeId AND cp.DateOfPrice = @LastCompetitorDateOfPrice
		WHERE
			nbc.IsSainsburysSite = 0
		UNION ALL
		SELECT
			nbc.IsGrocer,
			nbc.IsNearbyGrocer
		FROM
			@NearbyCompetitors nbc
			INNER JOIN dbo.SitePrice sp ON sp.SiteId = nbc.CompetitorSiteId AND sp.FuelTypeId = @FuelTypeId AND sp.DateOfCalc = @LastSitePriceDateOfCalc
		WHERE
			nbc.IsSainsburysSite = 1
	)
	SELECT 
		@CompetitorPriceCount = (SELECT COUNT(1) FROM NearbyPriceData),
		@GrocerPriceCount = (SELECT COUNT(1) FROM NearbyPriceData WHERE IsGrocer = 1),
		@NearbyGrocerPriceCount = (SELECT COUNT(1) FROM NearbyPriceData WHERE IsNearbyGrocer = 1);

	INSERT 
		@results
	SELECT 
		COALESCE(@CompetitorCount, 0),
		COALESCE(@GrocerCount, 0),
		COALESCE(@CompetitorPriceCount, 0),
		COALESCE(@GrocerPriceCount, 0),
		COALESCE(@NearbyGrocerCount, 0),
		COALESCE(@NearbyGrocerPriceCount, 0)

----DEBUG:START
--SELECT * FROM @results

--SELECT 
--	(SELECT COUNT(1) FROM @NearbyCompetitors) [CompetitorCount],
--	@ForDate [@ForDate], 
--	@FuelTypeId [@FuelTypeId],
--	@LastCompetitorDateOfPrice [@LastCompetitorDateOfPrice],
--	@LastSitePriceDateOfCalc [@LastSitePriceDateOfCalc]

--SELECT TOP 1 * FROM dbo.Site WHERE Id = @SiteId
----DEBUG:END

	RETURN
END
