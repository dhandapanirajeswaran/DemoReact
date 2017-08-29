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
	-- NOTE: FIX THIS SPEED ISSUE !!!

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-08-16'
--DECLARE	@DriveTime INT = 25
--DECLARE	@SiteId INT = 6164
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

	--
	-- find nearby Competitors 
	--
	-- conditions:
	--		(1) within X minutes drive time
	--		(2) Brand is NOT excluded
	--		(3) Competitor site is Active
	--		(4) Grocer OR non-Grocers
	--
	DECLARE @NearbyCompetitors TABLE (CompetitorSiteId INT PRIMARY KEY, CatNo INT, IsGrocer BIT)
	INSERT INTO @NearbyCompetitors
		SELECT
			stc.CompetitorId [CompetitorSiteId],
			compsite.CatNo [CatNo],
			compsite.isGrocer [IsGrocer]
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
		SELECT DISTINCT
			nbc.CatNo,
			nbc.IsGrocer
		FROM
			@NearbyCompetitors nbc
			INNER JOIN dbo.DailyPrice dp ON dp.CatNo = nbc.CatNo AND dp.FuelTypeId = @FuelTypeId
		WHERE
			dp.FuelTypeId = @FuelTypeId
			AND
			dp.DailyUploadId IN (
				SELECT 
					fu.Id
				FROM
					dbo.FileUpload fu
				WHERE
					fu.StatusId = @ImportProcessStatus_Success
					AND
					fu.UploadDateTime >= @ForDate
					AND
					fu.UploadDateTime < @ForDateNextDay
			)
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
----DEBUG:END

	RETURN
END
