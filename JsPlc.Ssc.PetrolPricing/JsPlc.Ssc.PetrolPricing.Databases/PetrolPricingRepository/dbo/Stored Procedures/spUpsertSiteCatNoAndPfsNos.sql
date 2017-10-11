CREATE PROCEDURE [dbo].[spUpsertSiteCatNoAndPfsNos]
	@SiteCatNoAndPfsNos XML
AS
BEGIN
	SET NOCOUNT ON;

	-- shred XML into table variable
	DECLARE @ImportTV TABLE(StoreNo INT, CatNo INT, PfsNo INT)
	INSERT INTO @ImportTV (StoreNo, CatNo, PfsNo)
	SELECT DISTINCT
		x.item.value('StoreNo[1]', 'INT') [StoreNo],
		x.item.value('CatNo[1]', 'INT') [CatNo],
		x.item.value('PfsNo[1]', 'INT') [PfsNo]
	FROM
		@SiteCatNoAndPfsNos.nodes('/*/SiteNumberImportViewModel') as x(item)

	--
	-- fix missing StoreNo (where possible) using CatNo
	--
	MERGE dbo.Site AS target
	USING
	(
		SELECT
			imp.StoreNo [NewStoreNo],
			imp.CatNo [NewCatNo]
		FROM
			@ImportTV imp
			INNER JOIN dbo.Site st ON st.CatNo = imp.CatNo
		WHERE
			imp.CatNo IS NOT NULL AND imp.CatNo > 0
	) AS source
	ON (target.CatNo = source.NewCatNo)
	WHEN MATCHED THEN
		UPDATE SET
			StoreNo = source.NewStoreNo;

	--
	-- import CatNo and PfsNo using StoreNo
	--
	MERGE dbo.Site AS target
		USING (
			SELECT
				st.StoreNo [StoreNo],
				st.CatNo [CatNo],
				st.PfsNo [PfsNo],
				NULLIF(imp.CatNo, 0) [ImportedCatNo],
				NULLIF(imp.PfsNo, 0) [ImportedPfsNo]
			FROM
				@ImportTV imp
				INNER JOIN dbo.Site st ON st.StoreNo = imp.StoreNo
			WHERE
				st.StoreNo IS NOT NULL AND st.StoreNo > 0
				AND
				imp.StoreNo IS NOT NULL AND imp.StoreNo > 0
		) AS source
		ON (target.StoreNo = source.StoreNo)
		WHEN MATCHED THEN
			UPDATE SET
				target.CatNo = COALESCE(source.ImportedCatNo, target.CatNo),
				target.PfsNo = COALESCE(source.ImportedPfsNo, target.PfsNo);

	--RETURN 0
END