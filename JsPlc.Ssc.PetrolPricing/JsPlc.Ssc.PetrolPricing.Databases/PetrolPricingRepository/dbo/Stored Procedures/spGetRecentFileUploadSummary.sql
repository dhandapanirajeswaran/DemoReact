CREATE PROCEDURE [dbo].[spGetRecentFileUploadSummary]
AS
	SET NOCOUNT ON
	SELECT
		ut.UploadTypeName [UploadTypeName],
		ut.Id [UploadTypeId],
		ips.Id [ImportStatusId],
		ips.Status [ImportStatus],
		fu.Id [FileUploadId],
		fu.OriginalFileName [OriginalFileName],
		fu.UploadDateTime [UploadDateTime],
		fu.UploadedBy [UploadedBy]
	FROM
		dbo.UploadType ut
		LEFT JOIN dbo.FileUpload fu ON fu.Id = (SELECT MAX(Id) FROM dbo.FileUpload WHERE UploadTypeId = ut.Id)
		LEFT JOIN dbo.ImportProcessStatus ips ON ips.Id = fu.StatusId
	ORDER BY
		ut.UploadTypeName ASC
RETURN 0
