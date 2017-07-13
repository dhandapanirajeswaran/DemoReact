CREATE FUNCTION [dbo].[fn_GetDriveTimePence]
(
	@FuelTypeId INT = 2,
	@DriveTime REAL = 30
)
RETURNS INT
AS
BEGIN
	DECLARE @DriveTimePence INT = (SELECT MAX(Markup) FROM dbo.DriveTimeMarkup WHERE FuelTypeId = @FuelTypeId AND DriveTime <= @DriveTime )

	RETURN COALESCE(@DriveTimePence,0)
END
