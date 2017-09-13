CREATE PROCEDURE [dbo].[spImportSystemSettings]
	@SettingsXml NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

DECLARE @ImportXml XML

----DEBUG:START
--DECLARE @SettingsXml NVARCHAR(MAX)

--DECLARE @TempTV TABLE(Data NVARCHAR(MAX))
--INSERT INTO @TempTV
--EXEC dbo.spExportSettings
--SELECT TOP 1 @SettingsXml = Data FROM @TempTV
----DEBUG:END

	SET @ImportXml = CONVERT(XML, @SettingsXml)

	--
	-- import [dbo].[SystemSettings] single record
	--
	;WITH ImportedSystemSettings AS (
		SELECT TOP 1
			x.item.value('DataCleanseFilesAfterDays[1]', 'int') [DataCleanseFilesAfterDays],
			x.item.value('MinUnleadedPrice[1]', 'int') [MinUnleadedPrice],
			x.item.value('MaxUnleadedPrice[1]', 'int') [MaxUnleadedPrice],
			x.item.value('MinDieselPrice[1]', 'int') [MinDieselPrice],
			x.item.value('MaxDieselPrice[1]', 'int') [MaxDieselPrice],
			x.item.value('MinSuperUnleadedPrice[1]', 'int') [MinSuperUnleadedPrice],
			x.item.value('MaxSuperUnleadedPrice[1]', 'int') [MaxSuperUnleadedPrice],
			x.item.value('MinUnleadedPriceChange[1]', 'int') [MinUnleadedPriceChange],
			x.item.value('MaxUnleadedPriceChange[1]', 'int') [MaxUnleadedPriceChange],
			x.item.value('MinDieselPriceChange[1]', 'int') [MinDieselPriceChange],
			x.item.value('MaxDieselPriceChange[1]', 'int') [MaxDieselPriceChange],
			x.item.value('MinSuperUnleadedPriceChange[1]', 'int') [MinSuperUnleadedPriceChange],
			x.item.value('MaxSuperUnleadedPriceChange[1]', 'int') [MaxSuperUnleadedPriceChange],
			x.item.value('MaxGrocerDriveTimeMinutes[1]', 'int') [MaxGrocerDriveTimeMinutes],
			x.item.value('PriceChangeVarianceThreshold[1]', 'int') [PriceChangeVarianceThreshold],
			x.item.value('SuperUnleadedMarkupPrice[1]', 'int') [SuperUnleadedMarkupPrice],
			x.item.value('DecimalRounding[1]', 'int') [DecimalRounding],
			x.item.value('EnableSiteEmails[1]', 'bit') [EnableSiteEmails],
			x.item.value('SiteEmailTestAddresses[1]', 'varchar(max)') [SiteEmailTestAddresses],
			x.item.value('FileUploadDatePicker[1]', 'bit') [FileUploadDatePicker],
			x.item.value('CompetitorMaxDriveTime[1]', 'int') [CompetitorMaxDriveTime]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/CommonSystemSettings[1]') as x(item)
	) 
	UPDATE	ss
	SET 
		ss.DataCleanseFilesAfterDays = imp.DataCleanseFilesAfterDays,
		ss.MinUnleadedPrice = imp.MinUnleadedPrice,
		ss.MaxUnleadedPrice = imp.MaxUnleadedPrice,
		ss.MinDieselPrice = imp.MinDieselPrice,
		ss.MaxDieselPrice = imp.MaxDieselPrice,
		ss.MinSuperUnleadedPrice = imp.MinSuperUnleadedPrice,
		ss.MaxSuperUnleadedPrice = imp.MaxSuperUnleadedPrice,
		ss.MinUnleadedPriceChange = imp.MinUnleadedPriceChange,
		ss.MaxUnleadedPriceChange = imp.MaxUnleadedPriceChange,
		ss.MinDieselPriceChange = imp.MinDieselPriceChange,
		ss.MaxDieselPriceChange = imp.MaxDieselPriceChange,
		ss.MinSuperUnleadedPriceChange = imp.MinSuperUnleadedPriceChange,
		ss.MaxSuperUnleadedPriceChange = imp.MaxSuperUnleadedPriceChange,
		ss.MaxGrocerDriveTimeMinutes = imp.MaxGrocerDriveTimeMinutes,
		ss.PriceChangeVarianceThreshold = imp.PriceChangeVarianceThreshold,
		ss.SuperUnleadedMarkupPrice = imp.SuperUnleadedMarkupPrice,
		ss.DecimalRounding = imp.DecimalRounding,
		ss.EnableSiteEmails = imp.EnableSiteEmails,
		ss.SiteEmailTestAddresses = imp.SiteEmailTestAddresses,
		ss.FileUploadDatePicker = imp.FileUploadDatePicker,
		ss.CompetitorMaxDriveTime = imp.CompetitorMaxDriveTime
	FROM 
		dbo.SystemSettings ss
		INNER JOIN ImportedSystemSettings imp ON 1=1

	--
	-- Import dbo.DriveTimeMarkups records
	--
	MERGE dbo.DriveTimeMarkup AS target
	USING (
		SELECT
			x.item.value('./@FuelTypeId[1]', 'int') [FuelTypeId],
			x.item.value('./@DriveTime[1]', 'int') [DriveTime],
			x.item.value('./@Markup[1]', 'int') [Markup]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/DriveTimeMarkups[1]/DriveTimeMarkup') as x(item)

	) AS source(FuelTypeId, DriveTime, Markup)
	ON (target.FuelTypeId = source.FuelTypeId AND target.DriveTime = source.DriveTime)
	WHEN MATCHED THEN
		UPDATE SET
			target.Markup = source.Markup
	WHEN NOT MATCHED BY target THEN
		INSERT (FuelTypeId, DriveTime, Markup)
		VALUES (
			source.FuelTypeId,
			source.DriveTime,
			source.Markup
		)
	WHEN NOT MATCHED BY source THEN
		DELETE;

	--
	-- Import dbo.Brands table
	--
	MERGE dbo.Brand AS target
	USING (
		SELECT
			x.item.value('./@BrandName[1]', 'varchar(100)') [BrandName]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/Brands[1]/Brand') as x(item)
	) AS source(BrandName)
	ON (target.BrandName = source.BrandName)
	WHEN NOT MATCHED BY target THEN
		INSERT (BrandName)
		VALUES (source.BrandName);

	--
	-- Import dbo.ExcludedBrands table
	--
	;WITH ImportedExcludedBrands AS (
		SELECT
			x.item.value('./@BrandName[1]', 'varchar(100)') [BrandName]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/ExcludeBrands[1]/ExcludedBrand') as x(item)
	)
	MERGE dbo.ExcludeBrands AS target
	USING (
		SELECT
			ieb.BrandName,
			br.Id
		FROM
			ImportedExcludedBrands ieb
			INNER JOIN dbo.Brand br ON br.BrandName = ieb.BrandName
	) AS source(BrandName, BrandId)
	ON (target.BrandName = source.BrandName)
	WHEN NOT MATCHED BY target THEN
		INSERT (BrandName, BrandId)
		VALUES (source.BrandName, source.BrandId)
	WHEN NOT MATCHED BY source THEN
		DELETE;

	--
	-- Import dbo.Grocers
	--
	;WITH ImportedGrocers AS (
		SELECT
			x.item.value('./@BrandName[1]', 'varchar(100)') [BrandName],
			x.item.value('./@IsSainsburys[1]', 'bit') [IsSainsburys]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/Grocers[1]/Grocer') as x(item)
	)
	MERGE dbo.Grocers AS target
	USING (
		SELECT
			ig.BrandName,
			ig.IsSainsburys,
			br.Id [BrandId]
		FROM
			ImportedGrocers ig
			INNER JOIN dbo.Brand br ON br.BrandName = ig.BrandName
	) AS source(BrandName, IsSainsburys, BrandId)
	ON (target.BrandName = source.BrandName)
	WHEN MATCHED THEN
		UPDATE SET
			target.IsSainsburys = source.IsSainsburys
	WHEN NOT MATCHED BY target THEN
		INSERT (BrandName, IsSainsburys, BrandId)
		VALUES (source.BrandName, source.IsSainsburys, source.BrandId)
	WHEN NOT MATCHED BY source THEN
		DELETE;

	--
	-- Import dbo.PriceFreezeEvent records
	--
	MERGE dbo.PriceFreezeEvent AS target
	USING (
		SELECT
			x.item.value('./@DateFrom[1]', 'datetime') [DateFrom],
			x.item.value('./@DateTo[1]', 'datetime') [DateTo],
			x.item.value('./@CreatedOn[1]', 'datetime') [CreatedOn],
			x.item.value('./@CreatedBy[1]', 'varchar(200)') [CreatedBy],
			x.item.value('./@IsActive[1]', 'bit') [IsActive]
		FROM
			@ImportXml.nodes('/PetrolPricing[1]/Settings[1]/PriceFreezeEvents[1]/PriceFreezeEvent') as x(item)
	) AS source(DateFrom, DateTo, CreatedOn, CreatedBy, IsActive)
	ON (target.DateFrom = source.DateFrom AND target.DateTo = source.DateTo)
	WHEN MATCHED THEN
		UPDATE SET
			target.CreatedOn = source.CreatedOn,
			target.CreatedBy = source.CreatedBy,
			target.IsActive = source.IsActive
	WHEN NOT MATCHED BY target THEN
		INSERT (DateFrom, DateTo, CreatedOn, CreatedBy, IsActive)
		VALUES (
			source.DateFrom,
			source.DateTo,
			source.CreatedOn,
			source.CreatedBy,
			source.IsActive
		)
	WHEN NOT MATCHED BY source THEN
		DELETE;

END

