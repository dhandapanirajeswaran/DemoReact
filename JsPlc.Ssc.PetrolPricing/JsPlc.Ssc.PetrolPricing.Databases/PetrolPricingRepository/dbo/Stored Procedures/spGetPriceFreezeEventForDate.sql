CREATE PROCEDURE [dbo].[spGetPriceFreezeEventForDate]
	@ForDate DATE
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
		pfe.IsActive [IsActive]
	FROM
		dbo.PriceFreezeEvent pfe
	WHERE
		pfe.IsActive = 1
		AND
		@ForDate BETWEEN pfe.DateFrom AND pfe.DateTo;

	RETURN 0
END
