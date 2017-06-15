CREATE FUNCTION dbo.fn_GetMarkupForDriveTime
(
	@FuelTypeId INT,
	@DriveTime INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Markup INT = 0

	SELECT TOP 1
		@Markup = dtm.Markup
	FROM 
		dbo.DriveTimeMarkup dtm
	WHERE
		dtm.FuelTypeId = @FuelTypeId
		AND
		dtm.DriveTime <= @DriveTime
	ORDER BY
		dtm.DriveTime DESC

	RETURN @Markup
END


