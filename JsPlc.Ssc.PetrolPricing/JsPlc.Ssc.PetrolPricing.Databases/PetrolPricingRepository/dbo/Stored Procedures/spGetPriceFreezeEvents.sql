CREATE PROCEDURE [dbo].[spGetPriceFreezeEvents]
AS
BEGIN
	SET NOCOUNT ON

	SELECT TOP 100
		pfe.PriceFreezeEventId [PriceFreezeEventId],
		pfe.DateFrom [DateFrom],
		pfe.DateTo [DateTo],
		DATEDIFF(DAY, pfe.DateFrom, pfe.DateTo)+ 1 [Days],
		pfe.CreatedOn [CreatedOn],
		pfe.CreatedBy [CreatedBy],
		pfe.IsActive [IsActive]
	FROM
		dbo.PriceFreezeEvent pfe
	ORDER BY
		pfe.DateFrom DESC

	RETURN 0
END
