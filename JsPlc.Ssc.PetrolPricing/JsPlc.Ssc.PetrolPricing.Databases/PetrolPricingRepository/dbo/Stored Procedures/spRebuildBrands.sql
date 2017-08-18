CREATE PROCEDURE [dbo].[spRebuildBrands]
AS
	SET NOCOUNT ON;

	--
	-- create missing Brands
	--
	INSERT INTO dbo.Brand
	SELECT DISTINCT st.Brand
	FROM dbo.Site st
	WHERE NOT EXISTS(SELECT TOP 1 NULL FROM dbo.Brand WHERE BrandName = st.Brand)

	--
	-- lookup BrandId for sites
	--
	UPDATE st
	SET BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = st.Brand)
	FROM dbo.Site st
	
RETURN 0
