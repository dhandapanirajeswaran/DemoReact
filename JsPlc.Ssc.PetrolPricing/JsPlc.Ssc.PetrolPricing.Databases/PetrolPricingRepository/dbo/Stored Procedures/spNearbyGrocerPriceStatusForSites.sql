CREATE PROCEDURE [dbo].[spNearbyGrocerPriceStatusForSites]
	@ForDate DATE,
	@DriveTime INT,
	@SiteIds VARCHAR(MAX)
AS
	SET NOCOUNT ON;

	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate)

		;WITH SiteList AS (
			SELECT
				st.id [SiteId]
			FROM
				dbo.tf_SplitIdsOnComma(@SiteIds) st
		),
		NearbyCompetitorFuelPrices AS (
		SELECT
			st.SiteId,
			stc.CompetitorId,
			lcp.FuelTypeId,
			lcp.ModalPrice 
		FROM
			SiteList st
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = st.SiteId
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
			INNER JOIN dbo.LatestCompPrice lcp ON lcp.CatNo = compsite.CatNo
		WHERE
			stc.DriveTime < @DriveTime
			AND
			lcp.UploadId IN (SELECT Id FROM dbo.FileUpload WHERE StatusId = 10 AND UploadDateTime >= @ForDate AND UploadDateTime < @ForDateNextDay)
	),
	CompetitorFuelsExists AS (
		SELECT
			sl.SiteId [SiteId],
			CASE WHEN EXISTS(SELECT TOP 1 NULL FROM NearbyCompetitorFuelPrices WHERE SiteId = sl.SiteId AND FuelTypeId = 2)
				THEN 1
				ELSE 0
			END [CompetitorUnleadedPriceExists],
			CASE WHEN EXISTS(SELECT TOP 1 NULL FROM NearbyCompetitorFuelPrices WHERE SiteId = sl.SiteId AND FuelTypeId = 6)
				THEN 1
				ELSE 0
			END [CompetitorDieselPriceExists]
		FROM
			SiteList sl
	)
	SELECT
		cfe.SiteId,
		cfe.CompetitorDieselPriceExists [HasNearbyCompetitorDieselPrice],
		cfe.CompetitorUnleadedPriceExists [HasNearbyCompetitorUnleadedPrice],
		cfe.CompetitorUnleadedPriceExists [HasNearbyCompetitorSuperUnleadedPrice] -- NOTE: Super-unleaded = Unleaded
	FROM 
		CompetitorFuelsExists cfe

RETURN 0