CREATE PROCEDURE dbo.spMarkPriceCacheOutdatedForDay
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE 
		dbo.PriceSnapshot
	SET 
		IsOutdated = 1,
		IsRecalcRequired = 1
	WHERE
		@ForDate BETWEEN DateFrom AND DateTo;
END