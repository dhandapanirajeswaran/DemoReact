CREATE FUNCTION dbo.fn_LastFileUploadForDate
(
	@ForDate DATE,
	@UploadTypeId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @FileUploadId INT;

	SELECT TOP 1
		@FileUploadId = fu.Id
	FROM
		dbo.FileUpload fu
	WHERE
		fu.UploadTypeId = @UploadTypeId
		AND
		fu.StatusId = 10 -- success
		AND
		fu.UploadDateTime >= @ForDate AND fu.UploadDateTime < DATEADD(DAY, 1, @ForDate)
	ORDER BY
		fu.UploadDateTime DESC

	RETURN @FileUploadId
END