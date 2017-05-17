CREATE PROCEDURE [dbo].[spGetQuarterlySiteAnalysisReport]
	@LeftFileUploadId INT,
	@RightFileUploadId INT
AS
	SET NOCOUNT ON

--DECLARE	@LeftFileUploadId INT = 21
--DECLARE	@RightFileUploadId INT = 22

	;WITH LeftSites AS (
		SELECT DISTINCT
			qua.CatNo, 
			qua.SiteName,
			qua.Ownership
		FROM
			dbo.QuarterlyUploadArchive qua
		WHERE
			qua.QuarterlyUploadId = @LeftFileUploadId
	),
	RightSites AS (
		SELECT DISTINCT
			qua.CatNo, 
			qua.SiteName,
			qua.Ownership
		FROM
			dbo.QuarterlyUploadArchive qua
		WHERE
			qua.QuarterlyUploadId = @RightFileUploadId
	),
	BothSites AS (
		SELECT
			ls.CatNo, 
			ls.SiteName
		FROM
			LeftSites ls
		UNION
		SELECT
			rs.CatNo,
			rs.SiteName
		FROM
			RightSites rs
	)
	--
	-- Resultset #1
	--
	SELECT
		both.CatNo [CatNo],
		both.SiteName [SiteName],
		CASE WHEN ls.CatNo IS NOT NULL
			THEN 1
			ELSE 0
		END [HasLeftSite],
		CASE WHEN rs.CatNo IS NOT NULL
			THEN 1
			ELSE 0
		END [HasRightSite],
		COALESCE(ls.Ownership, '') [LeftOwnership],
		COALESCE(rs.Ownership, '') [RightOwnership],
		CASE WHEN ls.Ownership IS NOT NULL AND rs.Ownership IS NOT NULL AND rs.Ownership <> ls.Ownership
			THEN 1
			ELSE 0
		END [HasOwnershipChanged],
		CASE WHEN ls.CatNo IS NULL AND rs.CatNo IS NOT NULL
			THEN 1
			ELSE 0
		END [WasSiteAdded],
		CASE WHEN ls.CatNo IS NOT NULL AND rs.CatNo IS NULL
			THEN 1
			ELSE 0
		END [WasSiteDeleted]
	FROM
		BothSites both
		LEFT JOIN LeftSites ls ON ls.CatNo = both.CatNo
		LEFT JOIN RightSites rs ON rs.CatNo = both.CatNo
	ORDER BY 
		both.SiteName

	--
	-- Recordset #2
	--
	SELECT
		(SELECT COUNT(1) FROM dbo.QuarterlyUploadArchive qua WHERE qua.QuarterlyUploadId = @LeftFileUploadId) [LeftTotalRecordCount],
		(SELECT COUNT(1) FROM dbo.QuarterlyUploadArchive qua WHERE qua.QuarterlyUploadId = @RightFileUploadId) [RightTotalRecordCount]

RETURN 0