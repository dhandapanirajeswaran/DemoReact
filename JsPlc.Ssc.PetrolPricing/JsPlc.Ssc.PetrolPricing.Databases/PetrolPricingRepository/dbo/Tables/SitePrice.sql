CREATE TABLE [dbo].[SitePrice] (
    [Id]              INT      IDENTITY (1, 1) NOT NULL,
    [SiteId]          INT      NOT NULL,
    [FuelTypeId]      INT      NOT NULL,
    [DateOfCalc]      DATETIME NOT NULL,
    [DateOfPrice]     DATETIME NOT NULL,
    [UploadId]        INT      NULL,
    [EffDate]         DATETIME NULL,
    [SuggestedPrice]  INT      NOT NULL,
    [OverriddenPrice] INT      NOT NULL,
    [CompetitorId]    INT      NULL,
    [Markup]          INT      NOT NULL,
    [IsTrailPrice]    BIT      NOT NULL,
    CONSTRAINT [PK_dbo.SitePrice] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SitePrice_CompetitorId_dbo.Site_SiteId] FOREIGN KEY ([CompetitorId]) REFERENCES [dbo].[Site] ([Id]),
    CONSTRAINT [FK_dbo.SitePrice_dbo.FuelType_FuelTypeId] FOREIGN KEY ([FuelTypeId]) REFERENCES [dbo].[FuelType] ([Id]),
    CONSTRAINT [FK_dbo.SitePrice_dbo.Site_SiteId] FOREIGN KEY ([SiteId]) REFERENCES [dbo].[Site] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_SiteId]
    ON [dbo].[SitePrice]([SiteId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FuelTypeId]
    ON [dbo].[SitePrice]([FuelTypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SiteId_FuelTypeId_DateOfCalc]
    ON [dbo].[SitePrice]([SiteId] ASC, [FuelTypeId] ASC, [DateOfCalc] ASC);

