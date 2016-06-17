CREATE TABLE [dbo].[SiteToCompetitor] (
    [Id]           INT  IDENTITY (1, 1) NOT NULL,
    [SiteId]       INT  NOT NULL,
    [CompetitorId] INT  NOT NULL,
    [Distance]     REAL NOT NULL,
    [DriveTime]    REAL NOT NULL,
    [Rank]         INT  NOT NULL,
	[IsExcluded]         INT  NOT NULL  DEFAULT(0),
    CONSTRAINT [PK_dbo.SiteToCompetitor] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SiteToCompetitor_dbo.Site_CompetitorId] FOREIGN KEY ([CompetitorId]) REFERENCES [dbo].[Site] ([Id]),
    CONSTRAINT [FK_dbo.SiteToCompetitor_dbo.Site_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Site] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_SiteId]
    ON [dbo].[SiteToCompetitor]([SiteId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CompetitorId]
    ON [dbo].[SiteToCompetitor]([CompetitorId] ASC);

