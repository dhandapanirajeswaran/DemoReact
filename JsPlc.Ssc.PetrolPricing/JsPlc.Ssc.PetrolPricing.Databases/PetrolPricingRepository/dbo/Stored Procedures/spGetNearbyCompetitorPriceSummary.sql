CREATE PROCEDURE [dbo].[spGetNearbyCompetitorPriceSummary]
	@ForDate DATE,
	@SiteIds VARCHAR(MAX),
	@DriveTime INT
AS
BEGIN
	SET NOCOUNT ON

	-- NOTE: REMEMBER TO ADD INDEX TO dbo.CompetitorPrice table !

----DEBUG:START
--DECLARE	@ForDate DATE = '2017-08-29'
----DECLARE	@SiteIds VARCHAR(MAX) = '6164'
--DECLARE @SiteIds VARCHAR(MAX) = '9,13,19,21,24,26,53,55,57,58,62,63,64,67,69,71,76,104,110,113,159,162,165,167,206,207,214,256,264,266,275,304,306,309,314,316,321,325,353,355,363,402,412,413,420,421,423,428,429,430,433,507,508,537,539,542,544,547,591,594,598,600,633,636,645,675,676,698,710,733,759,818,820,848,873,905,927,991,994,996,997,999,1000,1002,1040,1051,1053,1077,1078,1083,1085,1087,1097,1127,1128,1134,1160,1163,1167,1168,1169,1173,1201,1204,1211,1215,1223,1251,1259,1262,1263,1265,1268,1269,1274,1302,1304,1305,1313,1345,1349,1350,1395,1396,1403,1405,1437,1439,1440,1441,1494,1495,1503,1559,1560,1593,1599,1629,1637,1653,1696,1710,1740,1741,1748,1749,1750,1754,1793,1838,1847,1853,1854,1856,1895,1896,1910,1934,1976,1993,1995,1996,2037,2098,2105,2106,2107,2108,2137,2138,2141,2142,2148,2177,2180,2181,2222,2267,2309,2338,2389,2391,2395,2399,2426,2429,2447,2448,2449,2476,2491,2494,2604,2627,2732,2735,2756,2762,2791,2795,2798,2837,2849,2874,2923,2959,3047,3082,3084,3104,3117,3157,3180,3186,3187,3189,3192,3355,3374,3409,3417,3420,3421,3461,3513,3514,3543,3544,3551,3569,3603,3716,3735,3800,3801,3816,3895,3907,3925,3953,3954,3955,3969,4053,4174,4210,4227,4278,4319,4366,4367,4368,4425,4473,4526,4536,4565,4588,4654,4735,4813,4866,4919,4986,5014,5015,5037,5147,5148,5149,5155,5195,5254,5288,5471,5478,5716,5794,5796,5810,5910,5945,5957,6140,6177,6178,6179,6180,6181,6182,6183,6184,6185,6186,6187,6188,6189,6190,6191,6192,6193,6194,6195,6196,6197,6198'
--DECLARE	@DriveTime INT = 25
----DEBUG:END

	DECLARE @StartOfYesterday DATE = DATEADD(DAY, -1, @ForDate)

	DECLARE @SainsburysSiteFuels TABLE (SiteId INT NOT NULL, FuelTypeId INT NOT NULL, PRIMARY KEY(SiteId, FuelTypeId))
	INSERT INTO @SainsburysSiteFuels (SiteId, FuelTypeId)
	SELECT DISTINCT
		ids.Id [SiteId],
		ft.Id [FuelTypeId]
	FROM
		dbo.tf_SplitIdsOnComma(@SiteIds) ids
		CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft

	;WITH AllSiteFuelCompetitors AS (
		SELECT
			ssf.SiteId,
			ssf.FuelTypeId,
			stc.CompetitorId,
			compsite.IsGrocer
		FROM
			@SainsburysSiteFuels ssf
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = ssf.SiteId
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
		WHERE
			compsite.IsActive = 1
			AND
			stc.DriveTime < @DriveTime
			AND
			stc.IsExcluded = 0 -- Site Competitor is NOT excluded
			AND
			compsite.IsExcludedBrand = 0 -- Brand is NOT excluded
	)
	,UniqueCompetitors AS (
		SELECT DISTINCT 
			asfc.CompetitorId
		FROM
			AllSiteFuelCompetitors asfc
	),
	UniqueCompPrices AS (
		SELECT 
			uc.CompetitorId,
			ft.Id [FuelTypeId],
			CASE 
				WHEN EXISTS(SELECT TOP 1 NULL 
					FROM dbo.CompetitorPrice 
					WHERE SiteId = uc.CompetitorId 
						AND FuelTypeId = ft.Id 
						AND DateOfPrice = @StartOfYesterday
					)
				THEN 1
				ELSE 0
			END [PriceExists]
		FROM
			UniqueCompetitors uc
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (2, 6)) ft -- Unleaded and Diesel ONLY
	)
	SELECT
		ssf.SiteId,
		ssf.FuelTypeId,
		(
			SELECT COUNT(1) 
			FROM AllSiteFuelCompetitors 
			WHERE SiteId = ssf.SiteId AND FuelTypeId = ssf.FuelTypeId
		) [CompetitorCount],
		(
			SELECT COUNT(1)
			FROM AllSiteFuelCompetitors asfc1
			INNER JOIN UniqueCompPrices ucp1 ON ucp1.CompetitorId = asfc1.CompetitorId AND ucp1.FuelTypeId = asfc1.FuelTypeId
			WHERE ucp1.PriceExists = 1
				AND asfc1.SiteId = ssf.SiteId AND asfc1.FuelTypeId = ssf.FuelTypeId
		) [CompetitorPriceCount],
		(
			SELECT COUNT(1)
			FROM AllSiteFuelCompetitors
			WHERE SiteId = ssf.SiteId AND FuelTypeId = ssf.FuelTypeId AND IsGrocer =1
		) [GrocerCount],
		(
			SELECT COUNT(1)
			FROM AllSiteFuelCompetitors asfc2
			INNER JOIN UniqueCompPrices ucp2 ON ucp2.CompetitorId = asfc2.CompetitorId AND ucp2.FuelTypeId = asfc2.FuelTypeId
			WHERE ucp2.PriceExists = 1
				AND asfc2.SiteId = ssf.SiteId AND asfc2.FuelTypeId = ssf.FuelTypeId AND asfc2.IsGrocer = 1
		) [GrocerPriceCount]
	FROM
		@SainsburysSiteFuels ssf
END
