CREATE FUNCTION [dbo].[tf_NearbyCompetitorDataSummaryForSiteFuel]
(
	@ForDate DATE,
	@DriveTime INT,
	@SiteId INT,
	@FuelTypeId INT
)
RETURNS @results TABLE
(
	CompetitorCount INT,
	GrocerCount INT,
	CompetitorPriceCount INT,
	GrocerPriceCount INT
)
AS
BEGIN

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-09-05'
--DECLARE	@DriveTime INT = 25
--DECLARE	@SiteId INT = 1439
--DECLARE	@FuelTypeId INT = 2

--DECLARE @results TABLE
--(
--	CompetitorCount INT,
--	GrocerCount INT,
--	CompetitorPriceCount INT,
--	GrocerPriceCount INT
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

	DECLARE @LastCompetitorDateOfPrice DATE = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE DateOfPrice <= @ForDate)

	DECLARE @LastSitePriceDateOfCalc DATE = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE DateOfCalc <= DATEADD(DAY, -1, @ForDate))

	--
	-- find nearby Competitors 
	--
	-- conditions:
	--		(1) within X minutes drive time
	--		(2) Brand is NOT excluded
	--		(3) Competitor site is Active
	--		(4) Grocer OR non-Grocers
	--
	DECLARE @NearbyCompetitors TABLE (CompetitorSiteId INT PRIMARY KEY, CatNo INT, IsGrocer BIT, IsSainsburysSite BIT)
	INSERT INTO @NearbyCompetitors
		SELECT
			stc.CompetitorId [CompetitorSiteId],
			compsite.CatNo [CatNo],
			compsite.isGrocer [IsGrocer],
			compsite.IsSainsburysSite [IsSainsburysSite]
		FROM
			dbo.Site st
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = st.Id
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
		WHERE
			st.Id = @SiteId
			AND
			stc.DriveTime < @DriveTime -- within drive time
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
		@GrocerCount = (SELECT COUNT(1) FROM @NearbyCompetitors WHERE IsGrocer=1)

	--
	-- get nearby Competitor and Grocer price data (if any)
	--
	;WITH NearbyPriceData AS (
		SELECT
			nbc.IsGrocer
		FROM
			@NearbyCompetitors nbc
			INNER JOIN dbo.CompetitorPrice cp ON cp.SiteId = nbc.CompetitorSiteId AND cp.FuelTypeId = @FuelTypeId AND cp.DateOfPrice = @LastCompetitorDateOfPrice
		WHERE
			nbc.IsSainsburysSite = 0
		UNION ALL
		SELECT
			nbc.IsGrocer
		FROM
			@NearbyCompetitors nbc
			INNER JOIN dbo.SitePrice sp ON sp.SiteId = nbc.CompetitorSiteId AND sp.FuelTypeId = @FuelTypeId AND sp.DateOfCalc = @LastSitePriceDateOfCalc
		WHERE
			nbc.IsSainsburysSite = 1
	)
	SELECT 
		@CompetitorPriceCount = (SELECT COUNT(1) FROM NearbyPriceData),
		@GrocerPriceCount = (SELECT COUNT(1) FROM NearbyPriceData WHERE IsGrocer = 1);

	INSERT 
		@results
	SELECT 
		COALESCE(@CompetitorCount, 0),
		COALESCE(@GrocerCount, 0),
		COALESCE(@CompetitorPriceCount, 0),
		COALESCE(@GrocerPriceCount, 0);

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
