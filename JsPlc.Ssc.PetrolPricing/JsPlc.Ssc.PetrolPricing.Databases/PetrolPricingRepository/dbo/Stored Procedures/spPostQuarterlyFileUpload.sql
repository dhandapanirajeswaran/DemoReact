CREATE PROCEDURE [dbo].[spPostQuarterlyFileUpload]
AS
	SET NOCOUNT ON

	--
	-- Various post quarterly file upload steps
	--

	--
	-- Determine PriceMatchTypes for any newly added/updated [dbo].[Site] records
	--
	EXEC [dbo].[spSetSitePriceMatchTypeDefaults]

	--
	-- (re)init the Sainsbury's store information (psfno and StoreNo's)
	--
	EXEC [dbo].[spInitSainsburysStoreInformation]

	--
	-- look Site Grocer and ExcludedBrand attributes
	--
	EXEC dbo.spRebuildSiteAttributes @SiteId = NULL -- Note: ALL sites

RETURN 0
