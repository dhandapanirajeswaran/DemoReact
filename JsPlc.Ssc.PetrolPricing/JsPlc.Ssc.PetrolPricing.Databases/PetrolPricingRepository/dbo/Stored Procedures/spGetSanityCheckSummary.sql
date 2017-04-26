CREATE PROCEDURE [dbo].[spGetSanityCheckSummary]
AS
	SET NOCOUNT ON

SELECT
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.Site) THEN 1 ELSE 0 END) [SitesExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId = 10 AND UploadTypeId=1) THEN 1 ELSE 0 END) [DailyPriceFileUploadExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId = 10 AND UploadTypeId=2) THEN 1 ELSE 0 END) [QuarterlyFileUploadExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId = 10 AND UploadTypeId=3) THEN 1 ELSE 0 END) [LatestJSPriceFileUploadExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.FileUpload WHERE StatusId = 10 AND UploadTypeId=4) THEN 1 ELSE 0 END) [LatestCompPriceFileUploadExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.SitePrice) THEN 1 ELSE 0 END) [SitePricesExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.SiteToCompetitor) THEN 1 ELSE 0 END) [SiteCompetitorsExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.LatestCompPrice) THEN 1 ELSE 0 END) [LatestCompPricesExist],
	(CASE WHEN EXISTS(SELECT TOP 1 NULL FROM dbo.LatestPrice) THEN 1 ELSE 0 END) [LatestPricesExist]

RETURN 0
