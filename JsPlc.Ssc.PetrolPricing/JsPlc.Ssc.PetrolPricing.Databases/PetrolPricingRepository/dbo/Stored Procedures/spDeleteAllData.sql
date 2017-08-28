CREATE PROCEDURE [dbo].[spDeleteAllData]

AS
	SET NOCOUNT ON

	DECLARE @ErrorCode INT = 0
	DECLARE @ErrorMessage VARCHAR(MAX)

	BEGIN TRY
		BEGIN TRAN

			TRUNCATE TABLE dbo.CompetitorPrice
			TRUNCATE TABLE dbo.ExcludeBrands

			TRUNCATE TABLE dbo.EmailSendLog

			TRUNCATE TABLE dbo.DailyPrice
			TRUNCATE TABLE dbo.DailyUploadStaging
			TRUNCATE TABLE dbo.LatestCompPrice
			TRUNCATE TABLE dbo.ImportProcessError
			TRUNCATE TABLE dbo.LatestPrice

			TRUNCATE TABLE dbo.QuarterlyFileUpload
			TRUNCATE TABLE dbo.QuarterlyUploadArchive
			TRUNCATE TABLE dbo.QuarterlyUploadStaging

			TRUNCATE TABLE dbo.SiteEmail
			TRUNCATE TABLE dbo.SitePrice
			TRUNCATE TABLE dbo.SiteToCompetitor

			TRUNCATE TABLE dbo.PriceSnapshot
			TRUNCATE TABLE dbo.PriceSnapshotRow

			-- NOTE: TRUNCATE TABLE dbo.Site will not work 
			DELETE FROM dbo.Site

			-- NOTE: TRUNCATE TABLE dbo.FileUpload will not work
			DELETE FROM dbo.FileUpload

			-- Reseed the database tables

			DBCC CHECKIDENT ('CompetitorPrice', RESEED, 1)
			DBCC CHECKIDENT ('EmailSendLog', RESEED, 1)
			DBCC CHECKIDENT ('DailyPrice', RESEED, 1)
			DBCC CHECKIDENT ('DailyUploadStaging', RESEED, 1)
			DBCC CHECKIDENT ('LatestCompPrice', RESEED, 1)
			DBCC CHECKIDENT ('ImportProcessError', RESEED, 1)
			DBCC CHECKIDENT ('LatestPrice', RESEED, 1)
			DBCC CHECKIDENT ('QuarterlyFileUpload', RESEED, 1)
			DBCC CHECKIDENT ('QuarterlyUploadArchive', RESEED, 1)
			DBCC CHECKIDENT ('QuarterlyUploadStaging', RESEED, 1)
			DBCC CHECKIDENT ('SiteEmail', RESEED, 1)
			DBCC CHECKIDENT ('SitePrice', RESEED, 1)
			DBCC CHECKIDENT ('SiteToCompetitor', RESEED, 1)
			DBCC CHECKIDENT ('Site', RESEED, 1)
			DBCC CHECKIDENT ('FileUpload', RESEED, 1)
			DBCC CHECKIDENT ('PriceSnapshot', RESEED, 1)
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		SET @ErrorMessage = ERROR_MESSAGE()
		IF (@@TRANCOUNT > 0)
			ROLLBACK TRAN
		SET @ErrorCode = 1

	END CATCH

	-- resultset
	SELECT 
		@ErrorCode [ErrorCode],
		COALESCE(@ErrorMessage, '') [ErrorMessage]
RETURN 0
