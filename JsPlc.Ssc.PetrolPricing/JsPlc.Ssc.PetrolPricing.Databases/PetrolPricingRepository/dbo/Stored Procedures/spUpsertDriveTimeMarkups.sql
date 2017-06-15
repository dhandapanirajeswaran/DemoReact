CREATE PROCEDURE [dbo].[spUpsertDriveTimeMarkups]
	@DriveTimeMarkups XML
AS
BEGIN
	SET NOCOUNT ON

	MERGE
	dbo.DriveTimeMarkup AS target
	USING (
		SELECT
			x.item.value('FuelTypeId[1]', 'int') [FuelTypeId],
			x.item.value('DriveTime[1]', 'int') [DriveTime],
			x.item.value('Markup[1]', 'int') [Markup]
		FROM
			@DriveTimeMarkups.nodes('/*/DriveTimeMarkup') as x(item)
	) AS source(FuelTypeId, DriveTime, Markup)
	ON (source.FuelTypeId = target.FuelTypeId AND source.DriveTime = target.DriveTime)
	WHEN MATCHED 
		THEN UPDATE SET
			target.Markup = source.Markup
	WHEN NOT MATCHED BY target THEN
		INSERT (
			[FuelTypeId],
			[DriveTime],
			[Markup]
		)
		VALUES (
			source.FuelTypeId,
			source.DriveTime,
			source.Markup
		)
	WHEN NOT MATCHED BY source THEN
		DELETE;

END
RETURN 0
