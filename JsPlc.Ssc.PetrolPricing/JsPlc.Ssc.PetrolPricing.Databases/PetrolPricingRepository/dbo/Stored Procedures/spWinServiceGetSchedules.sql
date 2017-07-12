CREATE PROCEDURE [dbo].[spWinServiceGetSchedules]
AS
	SET NOCOUNT ON

	SELECT
		wss.WinServiceScheduleId,
		wss.IsActive,
		wss.WinServiceEventTypeId,
		wss.ScheduledFor,
		wss.LastPolledOn,
		wss.LastStartedOn,
		wss.LastCompletedOn,
		wss.WinServiceEventStatusId,
		COALESCE(wss.EmailAddress, '') [EmailAddress]
	FROM
		dbo.WinServiceSchedule wss

RETURN 0
