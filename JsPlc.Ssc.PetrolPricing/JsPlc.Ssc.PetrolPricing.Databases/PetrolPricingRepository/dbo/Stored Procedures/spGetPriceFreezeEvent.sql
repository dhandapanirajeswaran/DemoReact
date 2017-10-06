CREATE PROCEDURE [dbo].[spGetPriceFreezeEvent]
	@PriceFreezeEventId INT
AS
BEGIN
	SET NOCOUNT ON
	
	SELECT TOP 1
		pfe.PriceFreezeEventId [PriceFreezeEventId],
		pfe.DateFrom [DateFrom],
		pfe.DateTo [DateTo],
		DATEDIFF(DAY, pfe.DateFrom, pfe.DateTo) + 1 [Days],
		pfe.CreatedOn [CreatedOn],
		pfe.CreatedBy [CreatedBy],
		pfe.IsActive [IsActive],
		pfe.FuelTypeId [FuelTypeId]
	FROM
		dbo.PriceFreezeEvent pfe
	WHERE
		pfe.PriceFreezeEventId = @PriceFreezeEventId

	RETURN 0
END
