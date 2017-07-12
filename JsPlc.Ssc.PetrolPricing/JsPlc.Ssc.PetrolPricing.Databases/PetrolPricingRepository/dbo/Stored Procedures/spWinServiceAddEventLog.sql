CREATE PROCEDURE [dbo].[spWinServiceAddEventLog]
	@WinServiceScheduleId int,
	@WinServiceEventStatusId int,
	@Message varchar(200),
	@Exception nvarchar(max)
AS
	SET NOCOUNT ON

	INSERT INTO [dbo].[WinServiceEventLog]
           (
		   [CreatedOn],
           [WinServiceScheduleId],
           [WinServiceEventStatusId],
           [Message],
           [Exception]
		   )
     VALUES
           (
		   GetDate(),
           @WinServiceScheduleId,
           @WinServiceEventStatusId,
           @Message,
           @Exception
		   );
		   
RETURN 0
