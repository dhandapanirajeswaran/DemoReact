CREATE PROCEDURE [dbo].[spExportSettings]
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CommonSystemSettingsXml XML = (
		SELECT ss.*
		FROM dbo.SystemSettings ss
		FOR XML RAW('CommonSystemSettings'), ELEMENTS
	)

	DECLARE @DriveTimeMarkupXml XML = (
		SELECT
			dtm.FuelTypeId,
			dtm.DriveTime,
			dtm.Markup
		FROM dbo.DriveTimeMarkup dtm
		ORDER BY dtm.FuelTypeId, dtm.DriveTime
		FOR XML RAW('DriveTimeMarkup'), ROOT('DriveTimeMarkups')
	)

	DECLARE @BrandsXml XML = (
		SELECT
			br.BrandName
		FROM dbo.Brand br
		ORDER BY
			br.BrandName
		FOR XML RAW('Brand'), ROOT('Brands')
	)

	DECLARE @ExcludedBrandsXml XML = (
		SELECT
			eb.BrandName
		FROM dbo.ExcludeBrands eb
		ORDER BY
			eb.BrandName
		FOR XML RAW('ExcludedBrand'), ROOT('ExcludeBrands')
	)

	DECLARE @GrocersXml XML = (
		SELECT
			gro.BrandName,
			gro.IsSainsburys
		FROM dbo.Grocers gro
		ORDER BY
			gro.BrandName
		FOR XML RAW('Grocer'), ROOT('Grocers')
	)

	DECLARE @PriceFreezeEventsXml XML = (
		SELECT
			pfe.DateFrom,
			pfe.DateTo,
			pfe.CreatedOn,
			pfe.CreatedBy,
			pfe.IsActive
		FROM dbo.PriceFreezeEvent pfe
		ORDER BY
			pfe.DateFrom
		FOR XML RAW('PriceFreezeEvent'), ROOT('PriceFreezeEvents')
	)

	-- Resultset #1
	SELECT
		@CommonSystemSettingsXml ,
		@DriveTimeMarkupXml,
		@BrandsXml,
		@ExcludedBrandsXml, 
		@GrocersXml,
		@PriceFreezeEventsXml
		FOR XML RAW('Settings'), ROOT('PetrolPricing')
	RETURN 0
END
