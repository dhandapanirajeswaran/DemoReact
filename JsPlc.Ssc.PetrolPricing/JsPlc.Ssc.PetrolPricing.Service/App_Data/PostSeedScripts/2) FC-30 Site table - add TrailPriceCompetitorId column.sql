-- create TrailPriceCompetitorId column
IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'TrailPriceCompetitorId' AND Object_ID = Object_ID(N'Site')) 
BEGIN
	ALTER TABLE dbo.Site ADD
		TrailPriceCompetitorId int NULL
END

IF EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.[FK_dbo.Site_TrailPriceCompetitorId_dbo.Site_SiteId]')
   AND parent_object_id = OBJECT_ID(N'dbo.Site')
)
BEGIN
	ALTER TABLE [dbo].[Site] DROP CONSTRAINT [FK_dbo.Site_TrailPriceCompetitorId_dbo.Site_SiteId]
END

ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Site_TrailPriceCompetitorId_dbo.Site_SiteId] FOREIGN KEY([TrailPriceCompetitorId])
REFERENCES [dbo].[Site] ([Id])

ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_dbo.Site_TrailPriceCompetitorId_dbo.Site_SiteId]

-- create IsTrailPrice column
IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsTrailPrice' AND Object_ID = Object_ID(N'SitePrice')) 
BEGIN
	ALTER TABLE dbo.SitePrice ADD
		IsTrailPrice bit NOT NULL CONSTRAINT DF_SitePrice_IsTrialPrice DEFAULT 0

END

/****** Object:  Index [IX_SiteId_FuelTypeId_DateOfCalc]    Script Date: 11/02/2016 17:49:30 ******/
CREATE NONCLUSTERED INDEX [IX_SiteId_FuelTypeId_DateOfCalc] ON [dbo].[SitePrice]
(
	[SiteId] ASC,
	[FuelTypeId] ASC,
	[DateOfCalc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


