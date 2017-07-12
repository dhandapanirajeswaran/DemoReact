CREATE TABLE [dbo].[WinServiceSchedule]
(
	[WinServiceScheduleId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [IsActive] BIT NOT NULL, 
    [WinServiceEventTypeId] INT NOT NULL, 
    [ScheduledFor] DATETIME NOT NULL, 
    [LastPolledOn] DATETIME NULL, 
    [LastStartedOn] DATETIME NULL, 
    [LastCompletedOn] DATETIME NULL, 
    [WinServiceEventStatusId] INT NOT NULL, 
    [EmailAddress] VARCHAR(100) NULL
)
