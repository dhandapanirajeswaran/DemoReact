﻿CREATE FUNCTION [dbo].[fn_IsPriceFreezeActiveForDate]
(
	@ForDate DATE
)
RETURNS BIT
AS
BEGIN
	RETURN CASE
		WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.PriceFreezeEvent WHERE IsActive=1 AND @ForDate BETWEEN DateFrom AND DateTo) THEN 1
		ELSE 0
	END
END