CREATE PROCEDURE [dbo].[spArchiveQuarterlyUploadData]
AS
	SET NOCOUNT ON

	--
	-- Create record for each unique FileUpload in the Staging table
	--
	INSERT INTO
		[dbo].[QuarterlyFileUpload] (FileUploadId, IsActive)
	SELECT DISTINCT
		[QuarterlyUploadId],
		1
	FROM 
		[dbo].[QuarterlyUploadStaging]

	--
	-- Copy the records from the Staging table
	--
	INSERT INTO 
		[dbo].[QuarterlyUploadArchive]
	SELECT 
		  [QuarterlyUploadId]
		  ,[SainsSiteName]
		  ,[SainsSiteTown]
		  ,[SainsSiteCatNo]
		  ,[Rank]
		  ,[DriveDist]
		  ,[DriveTime]
		  ,[CatNo]
		  ,[Brand]
		  ,[SiteName]
		  ,[Addr]
		  ,[Suburb]
		  ,[Town]
		  ,[PostCode]
		  ,[Company]
		  ,[Ownership]
		  ,[AddSiteRow]
		  ,[AddSiteToCompRow]
	  FROM 
		[dbo].[QuarterlyUploadStaging]
RETURN 0
