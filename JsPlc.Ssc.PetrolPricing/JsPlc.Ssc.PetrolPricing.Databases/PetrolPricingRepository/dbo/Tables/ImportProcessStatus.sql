CREATE TABLE [dbo].[ImportProcessStatus] (
    [Id]     INT            NOT NULL,
    [Status] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.ImportProcessStatus] PRIMARY KEY CLUSTERED ([Id] ASC)
);

