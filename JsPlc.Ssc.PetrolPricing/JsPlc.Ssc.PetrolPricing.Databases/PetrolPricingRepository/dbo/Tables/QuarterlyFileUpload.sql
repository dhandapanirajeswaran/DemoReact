CREATE TABLE [dbo].[QuarterlyFileUpload]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [FileUploadId] INT NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT (1) 
)
