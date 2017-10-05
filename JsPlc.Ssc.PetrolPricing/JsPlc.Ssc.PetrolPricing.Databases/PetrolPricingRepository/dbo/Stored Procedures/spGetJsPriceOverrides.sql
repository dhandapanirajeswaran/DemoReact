CREATE PROCEDURE [dbo].[spGetJsPriceOverrides]
	@FileUploadId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		st.Id [SiteId],
		st.SiteName [SiteName],
		jpo.UploadId [FileUploadId],
		jpo.CatNo [CatNo],
		jpo.UnleadedIncrease [UnleadedIncrease],
		jpo.UnleadedAbsolute [UnleadedAbsolute],
		jpo.DieselIncrease [DieselIncrease],
		jpo.DieselAbsolute [DieselAbsolute],
		jpo.SuperUnleadedIncrease [SuperUnleadedIncrease],
		jpo.SuperUnleadedAbsolute [SuperUnleadedAbsolute]
	FROM
		dbo.JsPriceOverride jpo
		INNER JOIN dbo.Site st ON st.CatNo = jpo.CatNo
	WHERE
		jpo.UploadId = @FileUploadId
		AND
		st.IsActive = 1
END
	
