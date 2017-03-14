-- =============================================
--      Author: Garry Leeder
--     Created: 2017-03-06
--    Modified: 2017-03-06
-- Description:	Split comma separated string into table
-- =============================================
CREATE FUNCTION [dbo].[tf_SplitIdsOnComma]
(
	@IdCommaString VARCHAR(MAX)
)
RETURNS 
	@Slices TABLE (Id INT)
AS
BEGIN
	DECLARE @StartIndex INT = 0
	DECLARE @EndIndex INT = CHARINDEX(',', @IdCommaString)

	WHILE @EndIndex > 0
	BEGIN
		INSERT INTO @Slices 
		VALUES (SUBSTRING(@IdCommaString, @StartIndex, @EndIndex - @StartIndex))
		SET @StartIndex = @EndIndex + 1
		SET @EndIndex = CHARINDEX(',', @IdCommaString, @StartIndex+1)
	END
	INSERT INTO @Slices
	VALUES(SUBSTRING(@IdCommaString, @StartIndex, 1 + LEN(@IdCommaString) - @StartIndex))
	RETURN 
END