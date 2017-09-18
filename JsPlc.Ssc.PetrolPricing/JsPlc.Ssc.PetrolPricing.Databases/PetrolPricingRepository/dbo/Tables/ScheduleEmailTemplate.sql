CREATE TABLE [dbo].[ScheduleEmailTemplate]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ScheduleEmailType] INT NOT NULL, 
    [SubjectLine] VARCHAR(200) NOT NULL, 
    [ContactEmail] VARCHAR(100) NOT NULL, 
    [EmailBody] NVARCHAR(MAX) NULL
)
