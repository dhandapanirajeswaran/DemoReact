CREATE PROCEDURE [dbo].[spArchiveQuarterlyUploadData]
AS
	SET NOCOUNT ON

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
