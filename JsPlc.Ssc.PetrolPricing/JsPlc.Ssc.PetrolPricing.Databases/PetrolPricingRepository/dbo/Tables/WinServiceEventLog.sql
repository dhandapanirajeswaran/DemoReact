CREATE TABLE [dbo].[WinServiceEventLog]
(
	[WinServiceEventLogId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CreatedOn] DATETIME NOT NULL, 
    [WinServiceScheduleId] INT NOT NULL, 
    [WinServiceEventStatusId] INT NOT NULL, 
    [Message] VARCHAR(200) NOT NULL, 
    [Exception] NVARCHAR(MAX) NULL
)
