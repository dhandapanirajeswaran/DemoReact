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
--DECLARE @ForDate DATETIME = '2017-08-16 12:30:00'
--DECLARE @FileUploadId INT = 4
--DECLARE @MaxDriveTime INT = 25
----DEBUG:END

	--
	-- Constants
	--
	DECLARE @PriceReasonFlags_BasedOnUnleaded INT = 0x00000080

	DECLARE @FuelType_SuperUnleaded INT = 1
	DECLARE @FuelType_Unleaded INT = 2
	DECLARE @FuelType_Diesel INT = 6

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
			CROSS APPLY (SELECT Id FROM dbo.FuelType WHERE Id IN (@FuelType_Unleaded, @FuelType_Diesel)) ft -- NOTE: Unleaded and Diesel ONLY
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
				chp.PriceReasonFlags,
				chp.DriveTimeMarkup,
				chp.CompetitorCount,
				chp.CompetitorPriceCount,
				chp.GrocerCount,
				chp.GrocerPriceCount,
				chp.DriveTime
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
			cud.PriceReasonFlags,
			cud.DriveTimeMarkup,
			cud.CompetitorCount,
			cud.CompetitorPriceCount,
			cud.GrocerCount,
			cud.GrocerPriceCount
		FROM 
			CheapestUnleadedAndDiesel cud
		UNION ALL
		-- NOTE: Super-Unleaded = Unleaded + Markup
		SELECT
			unleaded.SiteId,
			@FuelType_SuperUnleaded [FuelTypeId], -- Super-Unleaded
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
			unleaded.PriceReasonFlags | @PriceReasonFlags_BasedOnUnleaded,
			dbo.fn_GetDriveTimePence(@FuelType_SuperUnleaded, unleaded.DriveTime), -- NOTE: Super-Unleaded DriveTime markup
			unleaded.CompetitorCount,
			unleaded.CompetitorPriceCount,
			unleaded.GrocerCount,
			unleaded.GrocerPriceCount
		FROM 
			CheapestUnleadedAndDiesel unleaded
		WHERE
			unleaded.FuelTypeId = @FuelType_Unleaded -- NOTE: base price on UNLEADED
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
				chp.PriceReasonFlags,
				chp.DriveTimeMarkup,
				chp.CompetitorCount,
				chp.CompetitorPriceCount,
				chp.GrocerCount,
				chp.GrocerPriceCount
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
				PriceReasonFlags,
				DriveTimeMarkup,
				CompetitorCount,
				CompetitorPriceCount,
				GrocerCount,
				GrocerPriceCount
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
				target.PriceReasonFlags = source.PriceReasonFlags,
				target.DriveTimeMarkup = source.DriveTimeMarkup,
				target.CompetitorCount = source.CompetitorCount,
				target.CompetitorPriceCount = source.CompetitorPriceCount,
				target.GrocerCount = source.GrocerCount,
				target.GrocerPriceCount = source.GrocerPriceCount
		WHEN NOT MATCHED BY target THEN
			INSERT (SiteId, FuelTypeId, DateOfCalc, DateOfPrice, UploadId, EffDate, SuggestedPrice, OverriddenPrice, CompetitorId, Markup, IsTrailPrice, PriceReasonFlags, DriveTimeMarkup, CompetitorCount, CompetitorPriceCount, GrocerCount, GrocerPriceCount)
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
				source.PriceReasonFlags,
				source.DriveTimeMarkup,
				source.CompetitorCount,
				source.CompetitorPriceCount,
				source.GrocerCount,
				source.GrocerPriceCount
			);

END