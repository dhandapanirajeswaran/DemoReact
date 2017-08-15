CREATE PROCEDURE [dbo].[spUpsertSiteCatNoAndPfsNos]
	@SiteCatNoAndPfsNos XML

AS
	SET NOCOUNT ON;

	;WITH Shredded AS (
		SELECT
			x.item.value('StoreNo[1]', 'INT') [StoreNo],
			x.item.value('CatNo[1]', 'INT') [CatNo],
			x.item.value('PfsNo[1]', 'INT') [PfsNo]
		FROM
			@SiteCatNoAndPfsNos.nodes('/*/SiteNumberImportViewModel') as x(item)
	),
	MatchedSites AS (
		SELECT
			st.*,
			sh.CatNo [NewCatNo],
			sh.PfsNo [NewPfsNo]
		FROM 
			Shredded sh
			INNER JOIN dbo.Site st ON st.StoreNo = sh.StoreNo
	)
	UPDATE
		MatchedSites
	SET
		CatNo = NewCatNo,
		PfsNo = CASE WHEN NewPfsNo > 0 THEN NewPfsNo ELSE PfsNo END;
RETURN 0
