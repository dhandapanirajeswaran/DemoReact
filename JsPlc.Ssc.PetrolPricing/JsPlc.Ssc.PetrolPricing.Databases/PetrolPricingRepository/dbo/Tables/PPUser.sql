CREATE TABLE [dbo].[PPUser] (
    [Id]                   int IDENTITY(1,1) PRIMARY KEY,
    [Email]                NVARCHAR (256) NULL,   
    [FirstName]         NVARCHAR (MAX) NULL,
    [LastName]           NVARCHAR (MAX) NULL, 
    [IsActive] BIT NOT NULL DEFAULT (1), 
    [CreatedOn] DATETIME NOT NULL DEFAULT (GetDate()), 
    [UpdatedOn] DATETIME NOT NULL DEFAULT (GetDate()), 
    [LastUsedOn] DATETIME NULL,
  
);

