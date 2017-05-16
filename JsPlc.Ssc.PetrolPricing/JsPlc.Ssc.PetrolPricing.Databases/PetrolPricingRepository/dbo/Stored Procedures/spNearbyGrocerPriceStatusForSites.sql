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
		NearbyCompetitors AS (
		SELECT
			st.SiteId,
			stc.CompetitorId,			
			compsite.CatNo,
			stc.DriveTime,
			compsite.Brand
		FROM
			SiteList st
			INNER JOIN dbo.SiteToCompetitor stc ON stc.SiteId = st.SiteId
			INNER JOIN dbo.Site compsite ON compsite.Id = stc.CompetitorId
			
		WHERE
			stc.DriveTime < @DriveTime
			AND
		    compsite.Brand in (select BrandName from Grocers)
		),
		NearbyCompetitorWithFuelPrices AS (
		SELECT
			st.SiteId,
			nc.CompetitorId,
			lcp.FuelTypeId,
			lcp.ModalPrice 
		FROM
			SiteList st
			INNER JOIN NearbyCompetitors nc ON nc.SiteId = st.SiteId		
			INNER JOIN dbo.LatestCompPrice lcp ON lcp.CatNo = nc.CatNo
		WHERE
			
			lcp.UploadId IN (SELECT Id FROM dbo.FileUpload WHERE StatusId = 10 AND UploadDateTime >= @ForDate AND UploadDateTime < @ForDateNextDay)
	),
	CompetitorFuelsExists AS (
		SELECT
			sl.SiteId [SiteId],
			CASE WHEN EXISTS(SELECT TOP 1 NULL FROM NearbyCompetitorWithFuelPrices WHERE SiteId = sl.SiteId AND FuelTypeId = 2)
				THEN 1
				ELSE 0
			END [CompetitorUnleadedPriceExists],
			CASE WHEN EXISTS(SELECT TOP 1 NULL FROM NearbyCompetitorWithFuelPrices WHERE SiteId = sl.SiteId AND FuelTypeId = 6)
				THEN 1
				ELSE 0
			END [CompetitorDieselPriceExists]
		FROM
			SiteList sl
	)
	SELECT
		cfe.SiteId,	
		
		CASE WHEN  cfe.CompetitorDieselPriceExists = 1 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END  [HasNearbyCompetitorDieselPrice],		
		CASE WHEN  cfe.CompetitorUnleadedPriceExists = 1 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END   [HasNearbyCompetitorUnleadedPrice],				
		CASE WHEN  cfe.CompetitorUnleadedPriceExists = 1 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END    [HasNearbyCompetitorSuperUnleadedPrice] ,-- NOTE: Super-unleaded = Unleaded,
		CASE WHEN  cfe.CompetitorDieselPriceExists = 0 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END  [HasNearbyCompetitorDieselWithOutPrice],
		CASE WHEN  cfe.CompetitorUnleadedPriceExists = 0 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END  [HasNearbyCompetitorUnleadedWithOutPrice],
		CASE WHEN  cfe.CompetitorUnleadedPriceExists = 0 AND EXISTS(select top 1 NULL from NearbyCompetitors nc where nc.SiteId=cfe.SiteId)
				THEN 1
				ELSE 0
		END  [HasNearbyCompetitorSuperUnleadedWithOutPrice] -- NOTE: Super-unleaded = Unleaded


	FROM 
		CompetitorFuelsExists cfe
		

RETURN 0