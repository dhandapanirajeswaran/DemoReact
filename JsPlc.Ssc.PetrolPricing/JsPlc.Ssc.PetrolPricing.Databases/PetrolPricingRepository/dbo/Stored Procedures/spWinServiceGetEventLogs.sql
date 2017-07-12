CREATE PROCEDURE [dbo].[spWinServiceGetEventLogs]
AS
		SET NOCOUNT ON

		SELECT TOP 50
			wsel.WinServiceEventLogId,
			wsel.CreatedOn,
			wsel.WinServiceScheduleId,
			wsel.WinServiceEventStatusId,
			wsel.Message,
			COALESCE(wsel.Exception, '') [Exception]
		FROM
			dbo.WinServiceEventLog wsel
		ORDER BY
			wsel.CreatedOn DESC

RETURN 0
