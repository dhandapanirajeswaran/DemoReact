-- =============================================
--      Author: Garry Leeder
--     Created: 2017-03-06
--    Modified: 2017-03-06
-- Description:	Replaces the last digit (units) of the supplied integer
-- =============================================
CREATE FUNCTION dbo.fn_ReplaceLastPriceDigit
(
	@Price INT,
	@Digit CHAR
)
RETURNS INT
AS
BEGIN
	DECLARE @Result INT = (@Price/10) * 10 + @Digit

	RETURN @Result
END