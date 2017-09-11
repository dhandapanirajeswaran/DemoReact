CREATE FUNCTION dbo.fn_IsDateOverlap
(
	@DateFrom1 DATETIME,
	@DateTo1 DATETIME,
	@DateFrom2 DATETIME,
	@DateTo2 DATETIME
)
RETURNS BIT
AS
BEGIN
	DECLARE @IsOverlapping BIT

	SET @IsOverlapping = CASE 
		WHEN @DateFrom2 < @DateTo1 AND @DateTo2 > @DateFrom1 THEN 1
		ELSE 0
	END

	RETURN @IsOverLapping
END
GO

