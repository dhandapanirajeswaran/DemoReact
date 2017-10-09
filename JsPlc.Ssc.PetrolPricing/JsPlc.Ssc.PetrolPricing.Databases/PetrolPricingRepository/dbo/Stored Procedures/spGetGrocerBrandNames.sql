CREATE PROCEDURE [dbo].[spGetGrocerBrandNames]
AS
BEGIN
	SET NOCOUNT ON

	SELECT
		gr.Id [GrocerId],
		gr.BrandName [BrandName],
		gr.BrandId [BrandId],
		CASE WHEN
			EXISTS(SELECT TOP 1 NULL FROM dbo.ExcludeBrands WHERE BrandId = gr.BrandId) THEN 1
			ELSE 0
		END [IsExcludedBrand]
	FROM
		dbo.Grocers gr

	RETURN 0
END
