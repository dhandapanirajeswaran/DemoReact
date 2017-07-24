CREATE PROCEDURE [dbo].[spRemoveAllSiteEmailAddresses]
AS
	SET NOCOUNT ON

	DELETE FROM dbo.SiteEmail;
RETURN 0
