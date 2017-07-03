CREATE PROCEDURE dbo.spResumePriceCacheForDay
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.PriceSnapshot SET IsActive=1 WHERE @ForDate BETWEEN DateFrom AND DateTo;
END