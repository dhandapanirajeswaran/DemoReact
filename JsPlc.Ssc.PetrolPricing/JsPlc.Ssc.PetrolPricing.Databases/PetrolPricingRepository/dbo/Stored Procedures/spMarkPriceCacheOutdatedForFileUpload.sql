CREATE PROCEDURE dbo.spMarkPriceCacheOutdatedForFileUpload
	@FileUploadId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @FileUpload_UploadDateTime DATE
	DECLARE @FileUpload_StatusId INT
	DECLARE @FileUpload_UploadTypeId INT
	SELECT TOP 1 
		@FileUpload_UploadDateTime = fu.UploadDateTime,
		@FileUpload_StatusId = fu.StatusId,
		@FileUpload_UploadTypeId = fu.UploadTypeId
	FROM 
		dbo.FileUpload  fu
	WHERE 
		fu.Id = @FileUploadId;

	DECLARE @ForAllDates BIT = CASE 
		WHEN @FileUpload_UploadTypeId = 1 THEN 0 -- Daily Price Data ?
		WHEN @FileUpload_UploadTypeId = 2 THEN 1 -- Quarterly Site Data ?
		WHEN @FileUpload_UploadTypeId = 3 THEN 1 -- Latest Js Price Data ?
		WHEN @FileUpload_UploadTypeId = 4 THEN 1 -- Latest Competitors Price Data ?
		ELSE
			0
		END;

	IF @FileUpload_StatusId = 10 AND @FileUpload_UploadDateTime IS NOT NULL
	BEGIN
		UPDATE 
			dbo.PriceSnapshot
		SET 
			IsOutdated = 1,
			IsRecalcRequired = 1
		WHERE
			@ForAllDates = 1
			OR
			@FileUpload_UploadDateTime BETWEEN DateFrom AND DateTo;
	END
END