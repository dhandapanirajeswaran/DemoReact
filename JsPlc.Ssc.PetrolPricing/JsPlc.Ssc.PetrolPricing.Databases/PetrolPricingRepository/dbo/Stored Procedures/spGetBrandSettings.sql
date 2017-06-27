CREATE PROCEDURE [dbo].[spGetBrandSettings]
AS
	SET NOCOUNT ON

	;WITH AllBrandNames AS (
		SELECT
			DISTINCT Brand
		FROM
			dbo.Site
		UNION
		SELECT 
			BrandName
		FROM
			dbo.Grocers
		UNION
		SELECT
			BrandName
		FROM
			dbo.ExcludeBrands
	)
	SELECT 
		abn.Brand [BrandName],
		CONVERT(BIT, CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.Grocers WHERE BrandName = abn.Brand) THEN 1 
		ELSE 0  
		END) [IsGrocer],
		CONVERT(BIT, CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.ExcludeBrands WHERE BrandName = abn.Brand) THEN 1
		ELSE 0
		END) [IsExcluded]
	FROM
		AllBrandNames abn
	ORDER BY
		abn.Brand
RETURN 0
