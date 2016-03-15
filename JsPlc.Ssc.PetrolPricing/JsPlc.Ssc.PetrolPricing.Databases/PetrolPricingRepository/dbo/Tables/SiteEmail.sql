CREATE TABLE [dbo].[SiteEmail] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [EmailAddress] NVARCHAR (MAX) NULL,
    [SiteId]       INT            NOT NULL,
    CONSTRAINT [PK_dbo.SiteEmail] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SiteEmail_dbo.Site_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Site] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_SiteId]
    ON [dbo].[SiteEmail]([SiteId] ASC);

