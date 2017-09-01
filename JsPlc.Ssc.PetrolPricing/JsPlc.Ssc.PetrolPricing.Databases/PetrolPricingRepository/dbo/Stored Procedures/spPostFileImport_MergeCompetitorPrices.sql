CREATE PROCEDURE [dbo].[spPostFileImport_MergeCompetitorPrices]
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START	
--DECLARE @ForDate DATE = '2017-09-01'
----DEBUG:END

	DECLARE @LatestCompUploadId INT = dbo.fn_LastFileUploadForDate(@ForDate, 4) -- Latest Comp Price Data
	DECLARE @DailyPrice_FileUploadId INT = dbo.fn_LastFileUploadForDate(@ForDate, 1) -- Daily Catalist File

	DECLARE @Latest_DateOfPrice DATE = CONVERT(DATE, @ForDate)

	-- NOTE: Daily Catalist file is for the previous day!
	DECLARE @Daily_DateOfPrice DATE = CONVERT(DATE, DATEADD(DAY, -1, @ForDate))

	--
	-- MERGE dbo.DailyPrice (for NON-Sainsburys sites) into dbo.CompetitorPrice table
	--
	IF @DailyPrice_FileUploadId IS NOT NULL 
	BEGIN
		;WITH CompSiteFuelsCTE AS (
			SELECT
				compsite.Id [CompSiteId],
				compsite.CatNo [CompCatNo],
				ft.Id [FuelTypeId]
			FROM
				dbo.Site compsite
				CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
			WHERE
				compsite.IsSainsburysSite = 0
		)
		MERGE
			dbo.CompetitorPrice AS target
			USING (
				SELECT
					csf.CompSiteId,
					csf.FuelTypeId,
					dp.ModalPrice,
					dp.Id [DailyPriceId]
				FROM
					CompSiteFuelsCTE csf
					INNER JOIN dbo.DailyPrice dp ON dp.CatNo = csf.CompCatNo AND dp.FuelTypeId = csf.FuelTypeId
				WHERE
					dp.DailyUploadId = @DailyPrice_FileUploadId
			) AS source(CompSiteId, FuelTypeId, ModalPrice, DailyPriceId)
			ON (target.SiteId = source.CompSiteId AND target.FuelTypeId = source.FuelTypeId AND target.DateOfPrice = @Daily_DateOfPrice)
			WHEN MATCHED AND target.LatestCompPriceId IS NULL THEN -- do NOT overwrite Latest Comp Prices!
				UPDATE SET
					target.ModalPrice = source.ModalPrice,
					target.DailyPriceId = source.DailyPriceId,
					target.LatestCompPriceId = NULL
			WHEN NOT MATCHED BY target THEN
				INSERT (SiteId, FuelTypeId, DateOfPrice, ModalPrice, DailyPriceId, LatestCompPriceId)
				VALUES (
					source.CompSiteId,
					source.FuelTypeId,
					@Daily_DateOfPrice,
					source.ModalPrice,
					source.DailyPriceId,
					NULL
				);
	END

	--
	-- MERGE dbo.LatestCompPrice into dbo.CompetitorPrice table
	--
	IF @LatestCompUploadId IS NOT NULL
	BEGIN

		;WITH CompSiteFuelsCTE AS (
			SELECT
				compsite.Id [CompSiteId],
				compsite.CatNo [CompCatNo],
				ft.Id [FuelTypeId]
			FROM
				dbo.Site compsite
				CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (1, 2, 6)) ft
			WHERE
				compsite.IsSainsburysSite = 0
		)
		MERGE
			dbo.CompetitorPrice AS target
		USING (
			SELECT
				csf.CompSiteId,
				csf.FuelTypeId,
				lcp.ModalPrice,
				lcp.Id [LatestCompPriceId]
			FROM
				CompSiteFuelsCTE csf
				INNER JOIN dbo.LatestCompPrice lcp ON lcp.CatNo = csf.CompCatNo AND lcp.FuelTypeId = csf.FuelTypeId
			WHERE
				lcp.UploadId = @LatestCompUploadId
		) AS source (CompSiteId, FuelTypeId, ModalPrice, LatestCompPriceId)
		ON (target.SiteId = source.CompSiteId AND target.FuelTypeId = source.FuelTypeId AND target.DateOfPrice = @Daily_DateOfPrice)
		WHEN MATCHED THEN
			UPDATE SET
				target.ModalPrice = source.ModalPrice,
				target.DailyPriceId = NULL,
				target.LatestCompPriceId = source.LatestCompPriceId
		WHEN NOT MATCHED BY target THEN
			INSERT (SiteId, FuelTypeId, DateOfPrice, ModalPrice, DailyPriceId, LatestCompPriceId)
			VALUES (
				source.CompSiteId,
				source.FuelTypeId,
				@Daily_DateOfPrice, -- NOTE: marked as yesterday for the pricing to find it
				source.ModalPrice,
				NULL,
				source.LatestCompPriceId
			);
	END

	RETURN 0
END
