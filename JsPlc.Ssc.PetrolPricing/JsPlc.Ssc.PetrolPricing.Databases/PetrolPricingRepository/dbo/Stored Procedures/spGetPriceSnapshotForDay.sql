CREATE PROCEDURE [dbo].[spGetPriceSnapshotForDay]
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

----DEBUG:START
--DECLARE	@ForDate DATE = GETDATE();
----DEBUG:END
----	
	--
	-- attempt to find PriceSnapshot for Date
	--
	DECLARE @PriceSnapshotId INT = (SELECT TOP 1 PriceSnapshotId FROM dbo.PriceSnapshot WHERE @ForDate BETWEEN DateFrom AND DateTo);
	--
	-- Safety check - see if there are any Active Sainsburys sites NOT captured on the PriceSnapshot
	--
	IF @PriceSnapshotId IS NOT NULL
	BEGIN
		-- Check if there are ANY Active JS sites NOT in the Snapshot
		IF EXISTS(SELECT TOP 1 NULL 
			FROM dbo.Site st
				LEFT JOIN dbo.PriceSnapshotRow psr ON psr.SiteId = st.Id AND psr.PriceSnapshotId = @PriceSnapshotId
			WHERE
				st.IsActive = 1 AND st.IsSainsburysSite = 1
				AND
				psr.SiteId IS NULL
		)
		BEGIN
			-- mark PriceSnapshot as outdated
			UPDATE dbo.PriceSnapshot SET IsOutdated = 1 WHERE PriceSnapshotId = @PriceSnapshotId;
		END
	END

	-- resultset
	SELECT TOP 1
		ps.PriceSnapshotId [PriceSnapshotId],
		ps.DateFrom [DateFrom],
		ps.DateTo [DateTo],
		ps.CreatedOn [CreatedOn],
		ps.UpdatedOn [UpdatedOn],
		ps.IsActive [IsActive],
		ps.IsOutdated [IsOutdated]
	FROM 
		dbo.PriceSnapshot ps
	WHERE
		ps.PriceSnapshotId = @PriceSnapshotId;

END
