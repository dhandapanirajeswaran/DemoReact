CREATE PROCEDURE [dbo].[spRebuildBrands]
AS
BEGIN
	SET NOCOUNT ON;

	--
	-- create missing Brands
	--
	INSERT INTO dbo.Brand
	SELECT DISTINCT st.Brand
	FROM dbo.Site st
	WHERE NOT EXISTS(SELECT TOP 1 NULL FROM dbo.Brand WHERE BrandName = st.Brand)

	--
	-- lookup BrandId for [dbo].[Site] table
	--
	UPDATE st
	SET BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = st.Brand)
	FROM dbo.Site st

	--
	-- lookup BrandIds in [dbo].[Grocers] table
	--
	update gr 
	set BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = gr.BrandName)
	FROM dbo.Grocers gr

	--
	-- lookup BrandIds in [dbo].[ExcludedBrands] table
	--
	update eb
	set BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = eb.BrandName)
	FROM dbo.ExcludeBrands eb

	--
	-- rebuild the [dbo].[Site] isGrocer and isExcludedBrands attributes
	--
	EXEC dbo.spRebuildSiteAttributes @SiteId = NULL; -- NOTE: all sites
	
	RETURN 0
END
