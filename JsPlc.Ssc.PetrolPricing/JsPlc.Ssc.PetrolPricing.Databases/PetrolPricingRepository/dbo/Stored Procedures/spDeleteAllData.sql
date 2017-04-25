CREATE PROCEDURE [dbo].[spDeleteAllData]
AS
	SET NOCOUNT ON

	DECLARE @ErrorCode INT = 0
	DECLARE @ErrorMessage VARCHAR(MAX)

	BEGIN TRY
		BEGIN TRAN

			TRUNCATE TABLE dbo.ExcludeBrands

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

			-- NOTE: TRUNCATE TABLE dbo.Site will not work 
			DELETE FROM dbo.Site

			-- NOTE: TRUNCATE TABLE dbo.FileUpload will not work
			DELETE FROM dbo.FileUpload
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
