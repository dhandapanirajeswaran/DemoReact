CREATE TABLE [dbo].[Grocers]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [BrandName] VARCHAR(200) NOT NULL, 
    [IsSainsburys] BIT NOT NULL
)

GO

CREATE INDEX [IDX_Grocers_BrandName] ON [dbo].[Grocers] ([BrandName])
