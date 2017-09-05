CREATE FUNCTION dbo.fn_LastCompetitorPriceId
(
	@SiteId INT,
	@FuelTypeId INT,
	@ForDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @CompetitorPriceId INT

	SELECT TOP 1
		@CompetitorPriceId = cp.Id
	FROM
		dbo.CompetitorPrice cp
	WHERE
		cp.SiteId = @SiteId
		AND
		cp.FuelTypeId = @FuelTypeId
		AND
		cp.DateOfPrice = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE SiteId = @SiteId AND FuelTypeId = @FuelTypeId AND ModalPrice > 0 AND DateOfPrice <= @ForDate)
	
	RETURN @CompetitorPriceId
END