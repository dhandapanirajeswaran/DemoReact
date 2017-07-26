CREATE PROCEDURE [dbo].[spUpsertSiteEmailAddreses]
	@EmailAddresses XML
AS
BEGIN
	SET NOCOUNT ON

	-- Safety - disable LIVE emails, user must re-enable in settings page
	UPDATE dbo.SystemSettings SET EnableSiteEmails = 0;


	;WITH ShreddedEmails AS (
		SELECT
			x.item.value('StoreNo[1]', 'INT') [StoreNo],
			x.item.value('StoreName[1]', 'NVARCHAR(200)') [StoreName],
			x.item.value('EmailAddress[1]', 'NVARCHAR(200)') [EmailAddress]
		FROM
			@EmailAddresses.nodes('/*/SiteEmailImportViewModel') as x(item)
	)
	MERGE
		dbo.SiteEmail AS target
		USING (
				SELECT 
					st.Id,
					se.StoreName,
					se.EmailAddress
				FROM 
					ShreddedEmails se
					INNER JOIN dbo.Site st ON st.StoreNo = se.StoreNo
				WHERE
					st.IsSainsburysSite = 1
	) AS source(SiteId, StoreName, EmailAddress)
	ON (source.SiteId = target.SiteId AND source.EmailAddress = target.EmailAddress)
	WHEN NOT MATCHED BY target THEN
		INSERT (
			EmailAddress,
			SiteId
		)
		VALUES (
			source.EmailAddress,
			source.SiteId
		);
END
--RETURN 0