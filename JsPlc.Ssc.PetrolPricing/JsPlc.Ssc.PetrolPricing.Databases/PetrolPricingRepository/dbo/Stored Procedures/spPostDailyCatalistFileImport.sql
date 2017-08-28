CREATE PROCEDURE [dbo].[spPostDailyCatalistFileImport]
	@FileUploadId INT,
	@FileUploadDateTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ForDate DATE = CONVERT(DATE, @FileUploadDateTime)

	EXEC dbo.spPostFileImport_MergeCompetitorPrices @ForDate = @ForDate

	RETURN 0
END
