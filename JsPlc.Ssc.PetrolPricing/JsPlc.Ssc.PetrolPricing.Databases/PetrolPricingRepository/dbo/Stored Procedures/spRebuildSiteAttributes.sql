CREATE PROCEDURE [dbo].[spRebuildSiteAttributes]
	@SiteId INT = NULL
AS
	SET NOCOUNT ON;

	UPDATE
		st
	SET
		IsGrocer = CASE 
			WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.Grocers WHERE BrandName = st.Brand) THEN 1 
			ELSE 0
		END,
		IsExcludedBrand = CASE
			WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.ExcludeBrands WHERE BrandName = st.Brand) THEN 1
			ELSE 0
		END

	FROM	
		dbo.Site st
	WHERE
		@SiteId IS NULL OR st.Id = @SiteId

RETURN 0
