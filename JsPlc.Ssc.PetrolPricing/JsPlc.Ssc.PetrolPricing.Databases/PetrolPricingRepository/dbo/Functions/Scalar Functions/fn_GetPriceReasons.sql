CREATE FUNCTION [dbo].[fn_GetPriceReasons]
(
	@PriceReasonFlags INT
)
RETURNS VARCHAR(1000)
AS
BEGIN
	DECLARE @Description VARCHAR(2000) = NULL
	SELECT @Description = COALESCE(@Description + ', ', '') + prf.Descript 
	FROM dbo.PriceReasonFlags prf 
	WHERE @PriceReasonFlags & prf.BitMask != 0

	RETURN @Description
END
