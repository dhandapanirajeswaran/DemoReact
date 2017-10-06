CREATE PROCEDURE [dbo].[spGetPriceFreezeEvents]
AS
BEGIN
	SET NOCOUNT ON

	-- expected a Maximum of 3 Fuel Types (Unleaded, Diesel and Super-Unleaded) for a Date
	SELECT
		pfe.PriceFreezeEventId [PriceFreezeEventId],
		pfe.DateFrom [DateFrom],
		pfe.DateTo [DateTo],
		DATEDIFF(DAY, pfe.DateFrom, pfe.DateTo)+ 1 [Days],
		pfe.CreatedOn [CreatedOn],
		pfe.CreatedBy [CreatedBy],
		pfe.IsActive [IsActive],
		pfe.FuelTypeId [FuelTypeId]
	FROM
		dbo.PriceFreezeEvent pfe
	ORDER BY
		pfe.DateFrom DESC

	RETURN 0
END
