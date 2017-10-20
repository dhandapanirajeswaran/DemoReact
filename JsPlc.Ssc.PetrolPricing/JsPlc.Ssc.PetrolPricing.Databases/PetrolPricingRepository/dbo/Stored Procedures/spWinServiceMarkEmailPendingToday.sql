CREATE PROCEDURE [dbo].[spWinServiceMarkEmailPendingToday]
	@UserName VARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @WinServiceScheduleId INT;
	DECLARE @WinServiceEventStatusId INT = 4; -- Success

	-- lookup DailyPriceEmail event record
	SELECT TOP 1
		@WinServiceScheduleId = wss.WinServiceScheduleId
	FROM
		dbo.WinServiceSchedule wss
	WHERE
		wss.WinServiceEventTypeId = 1 -- DailyPriceEmail

	IF @WinServiceScheduleId IS NOT NULL
	BEGIN
		--
		-- Create a log entry
		--
		INSERT INTO dbo.WinServiceEventLog (
			[CreatedOn],
			[WinServiceScheduleId],
			[WinServiceEventStatusId],
			[Message],
			[Exception] 
		)
		VALUES (
			GetDate(),
			@WinServiceScheduleId,
			@WinServiceEventStatusId,
			'Email LastCompletedOn was reset by ' + @UserName,
			NULL
		);

		--
		-- Clear the LastCompletedOn marker
		--
		UPDATE dbo.WinServiceSchedule SET LastCompletedOn = NULL WHERE WinServiceScheduleId = @WinServiceScheduleId;
	END
END
	