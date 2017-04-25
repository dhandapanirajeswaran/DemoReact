CREATE PROCEDURE [dbo].[spGetDiagnosticsRecordCounts]
AS
	SET NOCOUNT ON

	SELECT 'ContactDetails' [TableName], (SELECT COUNT(1) FROM dbo.ContactDetails) [RecordCount]
	UNION ALL SELECT 'DailyPrice', (SELECT COUNT(1) FROM dbo.DailyPrice)
	UNION ALL SELECT 'DailyUploadStaging', (SELECT COUNT(1) FROM dbo.DailyUploadStaging)
	UNION ALL SELECT 'EmailSendLog', (SELECT COUNT(1) FROM dbo.EmailSendLog)
	UNION ALL SELECT 'ExcludeBrands', (SELECT COUNT(1) FROM dbo.ExcludeBrands)
	UNION ALL SELECT 'FileUpload', (SELECT COUNT(1) FROM dbo.FileUpload)
	UNION ALL SELECT 'FuelType', (SELECT COUNT(1) FROM dbo.FuelType)
	UNION ALL SELECT 'Grocers', (SELECT COUNT(1) FROM dbo.Grocers)
	UNION ALL SELECT 'ImportProcessError', (SELECT COUNT(1) FROM dbo.ImportProcessError)
	UNION ALL SELECT 'ImportProcessStatus', (SELECT COUNT(1) FROM dbo.ImportProcessStatus)
	UNION ALL SELECT 'LatestCompPrice', (SELECT COUNT(1) FROM dbo.LatestCompPrice)
	UNION ALL SELECT 'LatestPrice', (SELECT COUNT(1) FROM dbo.LatestPrice)
	UNION ALL SELECT 'PPUser', (SELECT COUNT(1) FROM dbo.PPUser)
	UNION ALL SELECT 'PPUserPermissions', (SELECT COUNT(1) FROM dbo.PPUserPermissions)
	UNION ALL SELECT 'QuarterlyFileUpload', (SELECT COUNT(1) FROM dbo.QuarterlyFileUpload)
	UNION ALL SELECT 'QuarterlyUploadArchive', (SELECT COUNT(1) FROM dbo.QuarterlyUploadArchive)
	UNION ALL SELECT 'QuarterlyUploadStaging', (SELECT COUNT(1) FROM dbo.QuarterlyUploadStaging)
	UNION ALL SELECT 'Site', (SELECT COUNT(1) FROM dbo.Site)
	UNION ALL SELECT 'SiteEmail', (SELECT COUNT(1) FROM dbo.SiteEmail)
	UNION ALL SELECT 'SitePrice', (SELECT COUNT(1) FROM dbo.SitePrice)
	UNION ALL SELECT 'SiteToCompetitor', (SELECT COUNT(1) FROM dbo.SiteToCompetitor)
	UNION ALL SELECT 'SystemSettings', (SELECT COUNT(1) FROM dbo.SystemSettings)
	UNION ALL SELECT 'UploadType', (SELECT COUNT(1) FROM dbo.UploadType)
RETURN 0
