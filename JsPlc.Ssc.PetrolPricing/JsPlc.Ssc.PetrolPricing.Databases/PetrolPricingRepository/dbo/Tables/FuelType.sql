CREATE TABLE [dbo].[FuelType] (
    [Id]           INT            NOT NULL,
    [FuelTypeName] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.FuelType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

