-- =============================================
--      Author: Garry Leeder
--     Created: 2017-03-06
--    Modified: 2017-07-14
-- Description:	performs the 'Decimal Rounding' by replacing the last digit (if not -1)
--       Notes: 0 or NULL returns 0
-- =============================================
CREATE FUNCTION [dbo].[fn_ReplaceLastPriceDigit]
(
	@Price INT,
	@Digit INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Result INT = CASE 
		WHEN @Price = 0 OR @Price IS NULL THEN 0
		WHEN @Digit = -1 THEN @Price
		ELSE (@Price/10) * 10 + @Digit
	END

	RETURN @Result
END