CREATE PROCEDURE [dbo].[spValidateUploadAttempt]
	@UploadTypeId INT,
	@UploadDate DATE
AS
	SET NOCOUNT ON;

	DECLARE @ErrorMessage VARCHAR(100) = '';
	DECLARE @SuccessMessage VARCHAR(100) = ''

	--
	-- dbo.UploadType
	--
	DECLARE @UploadType_Daily_Price_Data INT = 1
	DECLARE @Uploadtype_Quarterly_Site_Date INT = 2
	DECLARE @UploadType_Latest_JS_Price_Data INT  = 3
	DECLARE @UploadType_Latest_Competitors_Price_Data INT = 4

	--
	-- dbo.ImportProcessStatus
	--
	DECLARE @ImportProcessStatus_Success INT = 10


	IF @UploadTypeId <> @Uploadtype_Quarterly_Site_Date
	BEGIN
		--
		-- Check that a Quarterly Site Data file has been uploaded
		--
		IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE UploadTypeId = @Uploadtype_Quarterly_Site_Date AND StatusId = @ImportProcessStatus_Success)
		BEGIN
			SET @ErrorMessage = 'No Quarterly Site Data file has been uploaded. Please upload one before any other files' 
		END
		ELSE
		BEGIN
			--
			-- check if a Newer Daily Price Data, Latest Js Price Data or Latest Competitors Price Data file exists
			--
			IF EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload 
				WHERE UploadTypeId IN (@UploadType_Daily_Price_Data, @UploadType_Latest_JS_Price_Data, @UploadType_Latest_Competitors_Price_Data) 
				AND StatusId = @ImportProcessStatus_Success
				AND CONVERT(DATE, UploadDateTime) > @UploadDate)
			BEGIN
				SET @ErrorMessage = 'A newer Daily Price Data, Latest Js Price Data or Latest Competitor Price Data file exists';
			END
		END
	END

	IF @ErrorMessage = ''
		SET @SuccessMessage = 'Upload attempt allowed';

	-- resultset
	SELECT
		@ErrorMessage [ErrorMessage],
		@SuccessMessage [SuccessMessage]

RETURN 0
