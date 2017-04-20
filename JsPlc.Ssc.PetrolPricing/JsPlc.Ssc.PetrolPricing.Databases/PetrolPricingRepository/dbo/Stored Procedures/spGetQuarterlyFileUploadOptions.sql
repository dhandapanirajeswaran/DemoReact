CREATE PROCEDURE [dbo].[spGetQuarterlyFileUploadOptions]
AS
	SET NOCOUNT ON

	SELECT
		qfu.FileUploadId [Id],
		fu.StoredFileName [Name]
	FROM
		[dbo].[QuarterlyFileUpload] qfu
		INNER JOIN dbo.FileUpload fu ON fu.Id = qfu.FileUploadId
	WHERE
		qfu.IsActive = 1
		AND
		fu.StatusId = 10
	ORDER BY
		fu.StoredFileName

RETURN 0
