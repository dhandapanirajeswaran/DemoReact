CREATE PROCEDURE [dbo].[spProcessSitePricing] 
	@SiteId INT,
	@ForDate DATETIME,
	@FileUploadId INT,
	@MaxDriveTime INT
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @SiteId INT = 6164
--DECLARE @ForDate DATETIME = '2017-08-14 12:30:00'
--DECLARE @FileUploadId INT = 5
--DECLARE @MaxDriveTime INT = 25
----DEBUG:END

	--
	-- Constants
	--
	DECLARE @PriceReasonFlags_BasedOnUnleaded INT = 0x00000080


	DECLARE @SuperUnleadedMarkup INT;
	SELECT TOP 1
		@SuperUnleadedMarkup = ss.SuperUnleadedMarkupPrice
	FROM
		dbo.SystemSettings ss

	;WITH SiteFuelsCombo AS (
		SELECT
			st.Id [SiteId],
			ft.Id [FuelTypeId]
		FROM
			dbo.Site st
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (2, 6)) ft -- NOTE: Unleaded and Diesel ONLY
		WHERE
			st.Id = @SiteId
	),
	CheapestUnleadedAndDiesel AS (
			SELECT
				chp.SiteId,
				chp.FuelTypeId,
				chp.DateOfCalc,
				chp.DateOfPrice,
				chp.SuggestedPrice,
				chp.UploadId,
				chp.Markup,
				chp.CompetitorId,
				chp.IsTrialPrice,
				chp.PriceReasonFlags
			FROM
				SiteFuelsCombo sfc
				CROSS APPLY dbo.tf_FindCheapestPrice(sfc.SiteId, sfc.FuelTypeId, @ForDate, @FileUploadId, @MaxDriveTime) chp
	),
	CheapestAllFuelPrices AS (
		-- Unleaded and Diesel
		SELECT 
			cud.SiteId,
			cud.FuelTypeId,
			cud.DateOfCalc,
			cud.DateOfPrice,
			cud.SuggestedPrice,
			cud.UploadId,
			cud.Markup,
			cud.CompetitorId,
			cud.IsTrialPrice,
			cud.PriceReasonFlags
		FROM 
			CheapestUnleadedAndDiesel cud
		UNION ALL
		-- NOTE: Super-Unleaded = Unleaded + Markup
		SELECT
			unleaded.SiteId,
			1 [FuelTypeId], -- Super-Unleaded
			unleaded.DateOfCalc,
			unleaded.DateOfPrice,
			CASE 
				WHEN unleaded.SuggestedPrice > 0 
				THEN unleaded.SuggestedPrice + @SuperUnleadedMarkup
				ELSE 0
			END [SuggestedPrice],
			unleaded.UploadId,
			unleaded.Markup,
			unleaded.CompetitorId,
			unleaded.IsTrialPrice,
			unleaded.PriceReasonFlags | @PriceReasonFlags_BasedOnUnleaded
		FROM 
			CheapestUnleadedAndDiesel unleaded
		WHERE
			unleaded.FuelTypeId = 2
	)

	MERGE
		dbo.SitePrice AS target
		USING (
			SELECT
				chp.SiteId,
				chp.FuelTypeId,
				chp.DateOfCalc,
				chp.DateOfPrice,
				chp.SuggestedPrice,
				chp.UploadId,
				chp.Markup,
				chp.CompetitorId,
				chp.IsTrialPrice,
				chp.PriceReasonFlags
			FROM
				CheapestAllFuelPrices chp
		) AS source (
				SiteId,
				FuelTypeId,
				DateOfCalc,
				DateOfPrice,
				SuggestedPrice,
				UploadId,
				Markup,
				CompetitorId,
				IsTrialPrice,
				PriceReasonFlags
		)
		ON (target.SiteId = source.SiteId AND target.FuelTypeId = source.FuelTypeId AND target.DateOfCalc = source.DateOfCalc)
		WHEN MATCHED THEN
			UPDATE SET
				target.DateOfPrice = source.DateOfPrice,
				target.SuggestedPrice = source.SuggestedPrice,
				target.UploadId = source.UploadId,
				target.Markup = source.Markup,
				target.CompetitorId = source.CompetitorId,
				target.IsTrailPrice = source.IsTrialPrice,
				target.PriceReasonFlags = source.PriceReasonFlags
		WHEN NOT MATCHED BY target THEN
			INSERT (SiteId, FuelTypeId, DateOfCalc, DateOfPrice, UploadId, EffDate, SuggestedPrice, OverriddenPrice, CompetitorId, Markup, IsTrailPrice, PriceReasonFlags)
			VALUES (
				source.SiteId,
				source.FuelTypeId,
				source.DateOfCalc,
				source.DateOfPrice,
				source.UploadId,
				NULL, -- EffDate
				source.SuggestedPrice,
				0, -- OverriddenPrice
				source.CompetitorId,
				source.Markup,
				source.IsTrialPrice,
				source.PriceReasonFlags
			);

END