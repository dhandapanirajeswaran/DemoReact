CREATE PROCEDURE [dbo].[spImportJsPriceOverrideData]
	@FileUploadId int,
	@JsPriceOverrides XML
AS
BEGIN
	SET NOCOUNT ON

	MERGE
		dbo.JsPriceOverride AS target
	USING (
		SELECT
			x.item.value('CatNo[1]', 'int') [CatNo],
			x.item.value('UnleadedIncrease[not(@xsi:nil = "true")][1]', 'int') [UnleadedIncrease],
			x.item.value('UnleadedAbsolute[not(@xsi:nil = "true")][1]', 'int') [UnleadedAbsolute],
			x.item.value('DieselIncrease[not(@xsi:nil = "true")][1]', 'int') [DieselIncrease],
			x.item.value('DieselAbsolute[not(@xsi:nil = "true")][1]', 'int') [DieselAbsolute],
			x.item.value('SuperUnleadedIncrease[not(@xsi:nil = "true")][1]', 'int') [SuperUnleadedIncrease],
			x.item.value('SuperUnleadedAbsolute[not(@xsi:nil = "true")][1]', 'int') [SuperUnleadedAbsolute]
		FROM 
			@JsPriceOverrides.nodes('/*/JsPriceOverrideDataModel') as x(item)
	) AS source(CatNo, UnleadedIncrease, UnleadedAbsolute, DieselIncrease, DieselAbsolute, SuperUnleadedIncrease, SuperUnleadedAbsolute)
	ON (target.UploadId = @FileUploadId AND source.CatNo = target.CatNo)
	WHEN MATCHED
		THEN UPDATE SET
			target.UnleadedIncrease = source.UnleadedIncrease,
			target.UnleadedAbsolute = source.UnleadedAbsolute,
			target.DieselIncrease = source.DieselIncrease,
			target.DieselAbsolute = source.DieselAbsolute,
			target.SuperUnleadedIncrease = source.SuperUnleadedIncrease,
			target.SuperUnleadedAbsolute = source.SuperUnleadedAbsolute
	WHEN NOT MATCHED BY target THEN
		INSERT (
			UploadId,
			CatNo,
			UnleadedIncrease,
			UnleadedAbsolute,
			DieselIncrease,
			DieselAbsolute,
			SuperUnleadedIncrease,
			SuperUnleadedAbsolute
		)
		VALUES (
			@FileUploadId,
			source.CatNo,
			source.UnleadedIncrease,
			source.UnleadedAbsolute,
			source.DieselIncrease,
			source.DieselAbsolute,
			source.SuperUnleadedIncrease,
			source.SuperUnleadedAbsolute
		);

	RETURN 0
END
