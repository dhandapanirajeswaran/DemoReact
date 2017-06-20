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

	-- constants
	DECLARE @HasNearbyGrocer_Flag TINYINT = 0x01; -- site has 1 or more nearby Grocers
	DECLARE @AllGrocersHavePriceData_Flag TINYINT = 0x02;


	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate);

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
			INNER JOIN dbo.Grocers gro ON gro.BrandName = compsite.Brand
		WHERE
			st.Id = @SiteId
			AND
			stc.DriveTime < @DriveTime
			AND
			compsite.IsActive = 1
			AND
			compsite.Brand NOT IN (SELECT BrandName FROM dbo.ExcludeBrands) -- ignore Excluded Brands

	IF EXISTS(SELECT TOP 1 NULL FROM @NearbyGrocerSites)
	BEGIN
		SET @NearbyGrocerStatus = @HasNearbyGrocer_Flag;

		--
		-- Check if ALL nearby grocers have price data for the date
		--
		SET @NearbyGrocerStatus = CASE WHEN NOT EXISTS(
			SELECT TOP 1 
				NULL 
			FROM 
				@NearbyGrocerSites ngs 
				INNER JOIN dbo.LatestCompPrice lcp ON ngs.CatNo = lcp.CatNo
			WHERE
				lcp.FuelTypeId = @FuelTypeId
				AND
				lcp.UploadId IN (SELECT Id FROM dbo.FileUpload WHERE StatusId = 10 AND UploadDateTime >= @ForDate AND UploadDateTime < @ForDateNextDay)
		) THEN @NearbyGrocerStatus 
		ELSE @NearbyGrocerStatus + @AllGrocersHavePriceData_Flag 
		END
	END

	-- result
	RETURN COALESCE(@NearbyGrocerStatus, 0)

END
