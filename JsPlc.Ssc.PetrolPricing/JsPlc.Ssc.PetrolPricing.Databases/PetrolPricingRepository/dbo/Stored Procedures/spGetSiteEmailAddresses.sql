CREATE PROCEDURE [dbo].[spGetSiteEmailAddresses]
	@SiteId INT = NULL
AS
	SET NOCOUNT ON

	SET @SiteId = COALESCE(@SiteId, 0)

	SELECT
		st.Id [SiteId],
		st.IsActive [IsSiteActive],
		st.StoreNo [StoreNo],
		st.CatNo [CatNo],
		st.PfsNo [PfsNo],
		st.SiteName [StoreName],
		COALESCE(se.EmailAddress, '') [EmailAddress]
	FROM
		dbo.Site st
		LEFT JOIN dbo.SiteEmail se ON se.SiteId = st.Id
	WHERE
		st.IsSainsburysSite =1
		AND (
			@SiteId = 0
			OR
			se.SiteId = @SiteId
		)
	ORDER BY
		St.SiteName

RETURN 0
