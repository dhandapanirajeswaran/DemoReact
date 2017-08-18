CREATE TABLE [dbo].[ExcludeBrands] (
    [Id]    INT IDENTITY(1,1) NOT NULL,
    [BrandName] NVARCHAR (MAX) NOT NULL,
    [BrandId] INT NOT NULL DEFAULT (0), 
    CONSTRAINT [PK_dbo.BrandName] PRIMARY KEY CLUSTERED ([Id] ASC)
);



