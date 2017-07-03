CREATE PROCEDURE dbo.spSuspendPriceCacheForDay
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.PriceSnapshot SET IsActive=0 WHERE @ForDate BETWEEN DateFrom AND DateTo;
END
