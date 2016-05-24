CREATE TABLE [dbo].[PPUser] (
    [Id]                   int IDENTITY(1,1) PRIMARY KEY,
    [Email]                NVARCHAR (256) NULL,   
    [FirstName]         NVARCHAR (MAX) NULL,
    [LastName]           NVARCHAR (MAX) NULL,
  
);

