CREATE PROCEDURE [dbo].[spWinServicePurgeEventLogs]
	@DaysAgo int
AS
	SET NOCOUNT ON

	IF @DaysAgo > 1
	BEGIN
		DECLARE @ForDate DATE = DATEADD(DAY, -@DaysAgo, GetDate());

		DELETE FROM dbo.WinServiceEventLog WHERE CreatedOn < @ForDate;
	END
RETURN 0
