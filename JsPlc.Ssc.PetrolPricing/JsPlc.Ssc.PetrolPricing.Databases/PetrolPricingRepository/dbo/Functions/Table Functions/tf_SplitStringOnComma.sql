CREATE FUNCTION [dbo].[tf_SplitStringOnComma]
(
	@CommaString VARCHAR(MAX)
)
RETURNS 
	@Slices TABLE (Value VARCHAR(MAX))
AS
BEGIN
	DECLARE @StartIndex INT = 0
	DECLARE @EndIndex INT = CHARINDEX(',', @CommaString)

	IF LEN(@CommaString) > 0 
	BEGIN
		WHILE @EndIndex > 0
		BEGIN
			INSERT INTO @Slices 
			VALUES (SUBSTRING(@CommaString, @StartIndex, @EndIndex - @StartIndex))
			SET @StartIndex = @EndIndex + 1
			SET @EndIndex = CHARINDEX(',', @CommaString, @StartIndex+1)
		END
		INSERT INTO @Slices
		VALUES(SUBSTRING(@CommaString, @StartIndex, 1 + LEN(@CommaString) - @StartIndex))
	END
	RETURN 
END