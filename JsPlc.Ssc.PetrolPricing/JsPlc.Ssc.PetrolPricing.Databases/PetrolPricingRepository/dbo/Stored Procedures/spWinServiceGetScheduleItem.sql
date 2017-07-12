CREATE PROCEDURE [dbo].[spWinServiceGetScheduleItem]
	@WinServiceScheduleId int
AS
	SET NOCOUNT ON

	SELECT TOP 1
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
	WHERE
		wss.WinServiceScheduleId = @WinServiceScheduleId

RETURN 0
