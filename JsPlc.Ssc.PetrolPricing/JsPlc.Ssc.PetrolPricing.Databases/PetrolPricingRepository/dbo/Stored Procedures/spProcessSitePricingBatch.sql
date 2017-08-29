CREATE PROCEDURE [dbo].[spProcessSitePricingBatch]
	@ForDate DATETIME,
	@FileUploadId INT,
	@MaxDriveTime INT,
	@SiteIds VARCHAR(MAX)
AS
BEGIN	
	SET NOCOUNT ON

----DEBUG:START
--DECLARE	@ForDate DATETIME = '2017-08-17'
--DECLARE	@FileUploadId INT = 6
--DECLARE	@MaxDriveTime INT = 26
--DECLARE	@SiteIds VARCHAR(MAX) = '6164,9'
----DEBUG:END

	DECLARE @SiteIdQueue TABLE(SiteId INT, RowIndex INT)
	INSERT INTO @SiteIdQueue
	SELECT
		ids.Id [SiteId],
		ROW_NUMBER() OVER (ORDER BY Id) [RowIndex]
	FROM
		dbo.tf_SplitIdsOnComma(@SiteIds) ids

	DECLARE @RowIndex INT = (SELECT MAX(RowIndex) FROM @SiteIdQueue)
	WHILE @RowIndex > 0
	BEGIN
		DECLARE @CurrentSiteId INT = (SELECT TOP 1 SiteId FROM @SiteIdQueue WHERE RowIndex = @RowIndex)

		-- Process Site pricing for Site for ALL Fuel grades
		EXEC dbo.spProcessSitePricing @SiteId = @CurrentSiteId, @ForDate = @ForDate, @FileUploadId = @FileUploadId, @MaxDriveTime = @MaxDriveTime;

		-- next SiteId in queue
		set @RowIndex = @RowIndex - 1
	END

--	RETURN 0
END
