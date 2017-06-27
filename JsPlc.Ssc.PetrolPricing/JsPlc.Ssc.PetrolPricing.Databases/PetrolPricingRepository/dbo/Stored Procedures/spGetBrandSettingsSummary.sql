CREATE PROCEDURE dbo.spGetBrandSettingsSummary
AS
BEGIN
	SET NOCOUNT ON;

	;WITH AllBrands AS (
		SELECT
			DISTINCT Brand
		FROM
			dbo.Site
	)
	SELECT
		(SELECT COUNT(1) FROM AllBrands) [BrandCount],
		(SELECT COUNT(1) FROM dbo.Grocers) [GrocerCount],
		(SELECT COUNT(1) FROM dbo.ExcludeBrands) [ExcludedCount]
END