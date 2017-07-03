CREATE PROCEDURE dbo.spDataCleansePriceSnapshots
	@DaysAgo INT
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE @DaysAgo INT = 2
----DEBUG:END

	IF @DaysAgo > 1
	BEGIN

		DECLARE @ForDate DATE = DATEADD(DAY, -@DaysAgo, GetDate());

		SELECT @ForDate

		DECLARE @PurgeIds TABLE(PriceSnapshotId INT);

		INSERT INTO @PurgeIds
		SELECT PriceSnapshotId FROM dbo.PriceSnapshot WHERE DateTo < @ForDate

		-- delete row records
		DELETE FROM dbo.PriceSnapshotRow WHERE PriceSnapshotId IN (SELECT PriceSnapshotId FROM @PurgeIds);

		-- delete header records
		DELETE FROM dbo.PriceSnapshot WHERE PriceSnapshotId IN (SELECT PriceSnapshotId FROM @PurgeIds);
	END
END
