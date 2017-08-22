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
--DECLARE	@SiteId INT = 6164
--DECLARE	@FuelTypeId INT = 2
----DEBUG:END

	-- constants
	DECLARE @HasNearbyGrocer_Flag TINYINT = 0x01; -- site has 1 or more nearby Grocers
	DECLARE @AllGrocersHavePriceData_Flag TINYINT = 0x02;

	DECLARE @ImportProcessStatus_Success INT = 10

	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate);
	DECLARE @yesterday DATE = DATEADD(DAY, -1, @forDate)
	DECLARE @yesterdayNextDay DATE = DATEADD(DAY, 1, @yesterday)

	--
	-- Find ALL competitor Grocers 
	--
	-- conditions:
	--		(1) within X minutes drive time
	--		(2) Brand is not excluded
	--		(3) Competitor site is active
	--
	DECLARE @NearbyGrocerSites TABLE (CompetitorSiteId INT PRIMARY KEY, CatNo INT);
	INSERT INTO @NearbyGrocerSites
		SELECT
			compsite.Id,
			compsite.CatNo
		FROM
			dbo.Site st
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = st.Id
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
		WHERE
			st.Id = @SiteId
			AND
			stc.DriveTime < @DriveTime
			AND
			compsite.IsActive = 1
			AND
			compsite.IsGrocer = 1
			AND
			compsite.IsExcludedBrand = 0 -- ignore Excluded Brands

	IF EXISTS(SELECT TOP 1 NULL FROM @NearbyGrocerSites)
	BEGIN
		SET @NearbyGrocerStatus = @HasNearbyGrocer_Flag;
		--
		-- Check if ALL nearby grocers have price data for the date
		--

		DECLARE @NearbyGrocerCount INT = (SELECT COUNT(1) FROM @NearbyGrocerSites);
		DECLARE @NearbyGrocerPriceDataCount INT = (SELECT COUNT(1)
			FROM 
				@NearbyGrocerSites gro
				INNER JOIN dbo.DailyPrice dp ON dp.CatNo = gro.CatNo AND dp.FuelTypeId = @FuelTypeId
			WHERE
				dp.FuelTypeId = @FuelTypeId
				AND
				dp.DailyUploadId IN (
					SELECT 
						MAX(ID)
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

		IF @NearbyGrocerCount = @NearbyGrocerPriceDataCount
		BEGIN
			SET @NearbyGrocerStatus = @NearbyGrocerStatus + @AllGrocersHavePriceData_Flag;
		END
	END

	-- result
	RETURN COALESCE(@NearbyGrocerStatus, 0)

	----DEBUG:START
	--SELECT @NearbyGrocerStatus [@NearbyGrocerStatus]
	----DEBUG:END
END
