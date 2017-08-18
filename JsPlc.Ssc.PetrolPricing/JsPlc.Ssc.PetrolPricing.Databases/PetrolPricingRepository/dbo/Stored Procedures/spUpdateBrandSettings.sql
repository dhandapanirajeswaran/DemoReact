﻿CREATE PROCEDURE dbo.spUpdateBrandSettings
	@Grocers NVARCHAR(MAX),
	@ExcludedBrands NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	--
	-- Add or Remove Grocers
	--
	MERGE dbo.Grocers AS target
	USING (
		SELECT 
			Value 
		FROM 
			dbo.tf_SplitStringOnComma(@Grocers)
	) AS Source(Brand)
	ON (source.Brand = target.BrandName)
	WHEN NOT MATCHED BY target THEN
		INSERT (BrandName, IsSainsburys)
		VALUES (source.Brand, 0)
	WHEN NOT MATCHED BY source THEN
		DELETE;
	--
	-- Add or Remove Excluded Brands
	--
	MERGE dbo.ExcludeBrands AS target
	USING (
		SELECT 
			Value 
		FROM 
			dbo.tf_SplitStringOnComma(@ExcludedBrands)
	) AS Source(Brand)
	ON (source.Brand = target.BrandName)
	WHEN NOT MATCHED BY target THEN
		INSERT (BrandName) VALUES(source.Brand)
	WHEN NOT MATCHED BY source THEN
		DELETE;

	--
	-- Update the BrandIds
	--
	UPDATE eb
	SET eb.BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = eb.BrandName)
	FROM dbo.ExcludeBrands eb
	--
END