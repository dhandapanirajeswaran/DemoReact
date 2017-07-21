CREATE PROCEDURE dbo.spGetCachedCalculatedPricesForDate
	@forDate DATE,
	@SiteIds VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @forDate DATE = '2017-06-29'
--DECLARE @SiteIds VARCHAR(MAX) = '9,13,19,21,24,26,53,55,57,58,62,63,64,67,69,71,76,104,110,113,159,162,165,167,206,207,214,256,264,266,275,304,306,309,314,316,321,325,353,355,363,402,412,413,420,421,423,428,429,430,433,507,508,537,539,542,544,547,591,594,598,600,633,636,645,675,676,698,710,733,759,818,820,848,873,905,927,991,994,996,997,999,1000,1002,1040,1051,1053,1077,1078,1083,1085,1087,1097,1127,1128,1134,1160,1163,1167,1168,1169,1173,1201,1204,1211,1215,1223,1251,1259,1262,1263,1265,1268,1269,1274,1302,1304,1305,1313,1345,1349,1350,1395,1396,1403,1405,1437,1439,1440,1441,1494,1495,1503,1559,1560,1593,1599,1629,1637,1653,1696,1710,1740,1741,1748,1749,1750,1754,1793,1838,1847,1853,1854,1856,1895,1896,1910,1934,1976,1993,1995,1996,2037,2098,2105,2106,2107,2108,2137,2138,2141,2142,2148,2177,2180,2181,2222,2267,2309,2338,2389,2391,2395,2399,2426,2429,2447,2448,2449,2476,2491,2494,2604,2627,2732,2735,2756,2762,2791,2795,2798,2837,2849,2874,2923,2959,3047,3082,3084,3104,3117,3157,3180,3186,3187,3189,3192,3355,3374,3409,3417,3420,3421,3461,3513,3514,3543,3544,3551,3569,3603,3716,3735,3800,3801,3816,3895,3907,3925,3953,3954,3955,3969,4053,4174,4210,4227,4278,4319,4366,4367,4368,4425,4473,4526,4536,4565,4588,4654,4735,4813,4866,4919,4986,5014,5015,5037,5147,5148,5149,5155,5195,5254,5288,5471,5478,5716,5794,5796,5810,5910,5945,5957,6140,6177,6178,6179,6180,6181,6182,6183,6184,6185,6186,6187,6188,6189,6190,6191,6192,6193,6194,6195,6196,6197,6198'
----DEBUG:END

	--
	-- Check if a cached copy of the results exist
	--
	DECLARE @PriceSnapshotId INT;
	DECLARE @PriceSnapshot_IsActive BIT;
	DECLARE @PriceSnapshot_IsOutdated BIT;
	SELECT TOP 1 
		@PriceSnapshotId = ps.PriceSnapshotId,
		@PriceSnapshot_IsActive = ps.IsActive,
		@PriceSnapshot_IsOutdated = ps.IsOutdated
	FROM 
		dbo.PriceSnapshot ps
	WHERE 
		@forDate BETWEEN ps.DateFrom AND ps.DateTo;

	IF @PriceSnapshotId IS NULL
	BEGIN
		-- 
		-- Create new [dbo].[PriceSnapshot] record for date (NOTE: IsActive=1 by default while inserting rows!)
		--
		INSERT INTO dbo.PriceSnapshot (DateFrom, DateTo, CreatedOn, UpdatedOn, IsActive, IsOutDated)
		VALUES (@forDate, @forDate, GETDATE(), GETDATE(), 0, 1)
		SET @PriceSnapshotId = SCOPE_IDENTITY()

		SET @PriceSnapshot_IsOutdated = 1;
	END

	IF @PriceSnapshot_IsOutdated = 1
	BEGIN
		UPDATE dbo.PriceSnapshot SET IsActive=0 WHERE PriceSnapshotId = @PriceSnapshotId;

		--
		-- Remove old records
		--
		DELETE FROM dbo.PriceSnapshotRow WHERE PriceSnapshotId = @PriceSnapshotId;

		--
		-- Calculate and store prices in cache
		--
		INSERT INTO dbo.PriceSnapshotRow
		EXEC dbo.spCalculateSitePricesForDate @ForDate, @SiteIds, @PriceSnapshotId

		--
		-- Mark the PriceSnapshot as being available for use and up-to-date
		--
		UPDATE dbo.PriceSnapshot SET UpdatedOn = GETDATE(), IsActive = 1, IsOutdated = 0, IsRecalcRequired=0 WHERE PriceSnapshotId = @PriceSnapshotId;

		SET @PriceSnapshot_IsActive = 1;
	END

	--
	-- Caching is currently being updated, so get non-cached values (slower)
	--
	IF @PriceSnapshotId IS NOT NULL AND @PriceSnapshot_IsActive = 0
	BEGIN
		EXEC dbo.spCalculateSitePricesForDate @ForDate, @SiteIds, NULL
	END

	--
	-- Cache is available, so return them
	--
	IF @PriceSnapshotId IS NOT NULL AND @PriceSnapshot_IsActive = 1
	BEGIN
		SELECT
			psr.*
		FROM 
			dbo.PriceSnapshotRow psr
		WHERE
			psr.PriceSnapshotId = @PriceSnapshotId
	END
END
