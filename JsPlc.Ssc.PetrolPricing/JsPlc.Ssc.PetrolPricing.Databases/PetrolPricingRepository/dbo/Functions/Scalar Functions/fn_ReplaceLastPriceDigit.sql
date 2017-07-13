-- =============================================
--      Author: Garry Leeder
--     Created: 2017-03-06
--    Modified: 2017-04-26
-- Description:	Replaces the last digit (units) of the supplied integer
--       Notes: 0 or NULL returns 0
-- =============================================
CREATE FUNCTION dbo.fn_ReplaceLastPriceDigit
(
	@Price INT,
	@Digit INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Result INT = CASE 
		WHEN @Price = 0 OR @Price IS NULL OR @Digit = -1
			THEN 0
		ELSE (@Price/10) * 10 + @Digit
	END

	RETURN @Result
END