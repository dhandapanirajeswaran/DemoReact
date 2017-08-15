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
	)
	MERGE
		dbo.Site AS target
		USING (
			SELECT DISTINCT
				sh.StoreNo,
				sh.CatNo,
				sh.PfsNo
			FROM
				Shredded sh
			WHERE
				sh.StoreNo > 0
				AND
				sh.CatNo > 0
		) AS source(StoreNo, CatNo, PfsNo)
		ON (source.StoreNo = target.StoreNo)
		WHEN MATCHED THEN
			UPDATE SET
				CatNo = source.CatNo,
				PfsNo = CASE WHEN source.PfsNo > 0 THEN source.PfsNo ELSE target.PfsNo END;

RETURN 0
