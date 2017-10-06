CREATE FUNCTION [dbo].[fn_IsPriceFreezeActiveForDate]
(
	@ForDate DATE,
	@FuelTypeId INT
)
RETURNS BIT
AS
BEGIN
	RETURN CASE
		WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.PriceFreezeEvent WHERE IsActive=1 AND FuelTypeId = @FuelTypeId AND @ForDate BETWEEN DateFrom AND DATEADD(DAY, -1, DateTo)) THEN 1
		ELSE 0
	END
END
