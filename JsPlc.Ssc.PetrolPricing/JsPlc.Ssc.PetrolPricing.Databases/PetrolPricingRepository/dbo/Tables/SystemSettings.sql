CREATE TABLE [dbo].[SystemSettings]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataCleanseFilesAfterDays] INT NOT NULL DEFAULT (60), 
    [LastDataCleanseFilesOn] DATETIME NULL
)
