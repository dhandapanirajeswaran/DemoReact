CREATE PROCEDURE [dbo].[spWinServiceUpsertSchedule]
	@WinServiceScheduleId		INT,
	@IsActive					BIT,
	@WinServiceEventTypeId		INT,
	@ScheduledFor				DATETIME,
	@LastPolledOn				DATETIME, 
	@LastStartedOn				DATETIME ,
	@LastCompletedOn			DATETIME,
	@WinServiceEventStatusId	INT,
	@EmailAddress				VARCHAR(100)
AS
	SET NOCOUNT ON

	---- make sure next schedule date is in the future
	--IF @ScheduledFor < GETDATE()
	--	SET @ScheduledFor = DATEADD(DAY, 1, @ScheduledFor);

	IF @WinServiceScheduleId = 0
	BEGIN
		INSERT INTO dbo.WinServiceSchedule
		(
			IsActive,
			WinServiceEventTypeId,
			ScheduledFor,
			LastPolledOn,
			LastStartedOn,
			LastCompletedOn,
			WinServiceEventStatusId,
			EmailAddress
		)
		VALUES(
			@IsActive,
			@WinServiceEventTypeId,
			@ScheduledFor,
			@LastPolledOn,
			@LastStartedOn,
			@LastCompletedOn,
			@WinServiceEventStatusId,
			@EmailAddress
		);
		SET @WinServiceScheduleId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE dbo.WinServiceSchedule
		SET
			WinServiceEventTypeId = @WinServiceEventTypeId,
			IsActive = @IsActive,
			ScheduledFor = @ScheduledFor,
			LastPolledOn = @LastPolledOn,
			LastStartedOn = @LastStartedOn,
			LastCompletedOn = @LastCompletedOn,
			WinServiceEventStatusId = @WinServiceEventStatusId,
			EmailAddress = @EmailAddress
		WHERE
			WinServiceScheduleId = @WinServiceScheduleId;
	END

	-- resultset
	SELECT
		wss.WinServiceScheduleId,
		wss.IsActive,
		wss.WinServiceEventTypeId,
		wss.ScheduledFor,
		wss.LastPolledOn,
		wss.LastStartedOn,
		wss.LastCompletedOn,
		wss.WinServiceEventStatusId,
		COALESCE(wss.EmailAddress, '') [EmailAddress],
		wses.EventStatusName,
		wset.EventTypeName
	FROM
		dbo.WinServiceSchedule wss
		INNER JOIN dbo.WinServiceEventStatus wses ON wses.WinServiceEventStatusId = wss.WinServiceEventStatusId
		INNER JOIN dbo.WinServiceEventType wset ON wset.WinServiceEventTypeId = wss.WinServiceEventTypeId
	WHERE
		wss.WinServiceScheduleId = @WinServiceScheduleId;

RETURN 0
