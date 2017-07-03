CREATE PROCEDURE dbo.spGetPriceSnapshotForDay
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON;

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
		@ForDate BETWEEN ps.DateFrom AND ps.DateTo;

END