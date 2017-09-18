CREATE PROCEDURE [dbo].[spGetLastSitePricesReport]
	@ForDate DATE,
	@IncludeSainsburys BIT,
	@IncludeCompetitors BIT
AS
BEGIN

----DEBUG:START
--DECLARE @ForDate DATE = GETDATE()
--DECLARE	@IncludeSainsburys BIT = 1
--DECLARE	@IncludeCompetitors BIT = 0
----DEBUG:END

	DECLARE @SitesTV TABLE (SiteId INT)
	INSERT INTO @SitesTV
	SELECT
		st.Id [SiteId]
	FROM
		dbo.Site st
	WHERE
		(st.IsSainsburysSite = 1 AND @IncludeSainsburys =1)
		OR
		(st.IsSainsburysSite = 0 AND @IncludeCompetitors = 1)
	ORDER BY
		st.SiteName

	---- resultset #1
	SELECT
		st.Id [SiteId],
		st.SiteName [SiteName],
		st.IsSainsburysSite [IsSainsburysSite],
		st.IsActive [IsActive],
		st.CatNo [CatNo],
		st.PfsNo [PfsNo]
	FROM
		@SitesTV stv
		INNER JOIN dbo.Site st ON st.Id = stv.SiteId
	ORDER BY
		st.SiteName


	-- resultset #2
	;WITH LastSainsburysPrices AS (
		SELECT
			st.Id [SiteId],
			ft.FuelTypeId [FuelTypeId],
			CASE
				WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
				WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
				ELSE 0
			END [ModalPrice],
			CONVERT(DATE, sp.DateOfCalc) [LastPriceDate]
		FROM
			@SitesTV stv
			INNER JOIN dbo.Site st ON st.Id = stv.SiteId
			CROSS APPLY (SELECT Id [FuelTypeId] FROM dbo.FuelType WHERE Id IN (1,2,6)) ft
			CROSS APPLY (
				SELECT 
					SuggestedPrice,
					OverriddenPrice,
					DateOfCalc
				FROM 
					dbo.SitePrice
				WHERE
					SiteId = st.Id
					AND
					FuelTypeId = ft.FuelTypeId
					AND
					DateOfCalc = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE SiteId = st.Id AND FuelTypeId = ft.FuelTypeId)
			) sp
	),
	LastCompetitorPrices AS (
		SELECT
			st.Id [SiteId],
			ft.FuelTypeId [FuelTypeId],
			COALESCE(cp.ModalPrice, 0) [ModalPrice],
			CONVERT(DATE, cp.DateOfPrice) [LastPriceDate]
		FROM
			@SitesTV stv
			INNER JOIN dbo.Site st ON st.Id = stv.SiteId
			CROSS APPLY (SELECT Id [FuelTypeId] FROM dbo.FuelType WHERE Id IN (1,2,6)) ft
			CROSS APPLY (
				SELECT *
				FROM
					dbo.CompetitorPrice
				WHERE
					SiteId = st.Id
					AND
					FuelTypeId = ft.FuelTypeId
					AND
					DateOfPrice = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE SiteId = st.Id AND FuelTypeId = ft.FuelTypeId)
			) cp
	),
	AllSitePrices AS (
		SELECT
			lsp.SiteId,
			lsp.FuelTypeId,
			lsp.ModalPrice,
			lsp.LastPriceDate
		FROM 
			LastSainsburysPrices lsp
		UNION ALL
		SELECT
			lcp.SiteId,
			lcp.FuelTypeId,
			lcp.ModalPrice,
			lcp.LastPriceDate
		FROM
			LastCompetitorPrices lcp
	)
	SELECT
		stv.SiteId [SiteId],
		ft.FuelTypeId [FuelType],
		COALESCE(asp.ModalPrice, 0) [ModalPrice],
		asp.LastPriceDate [LastPriceDate]
	FROM
		@SitesTV stv
		CROSS APPLY (SELECT Id [FuelTypeId] FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
		LEFT JOIN AllSitePrices asp ON asp.SiteId = stv.SiteId AND asp.FuelTypeId = ft.FuelTypeId

END
