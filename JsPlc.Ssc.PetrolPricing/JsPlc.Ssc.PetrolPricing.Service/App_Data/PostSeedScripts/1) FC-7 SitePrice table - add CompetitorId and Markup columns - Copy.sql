-- create CompetitorId column
IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'CompetitorId' AND Object_ID = Object_ID(N'SitePrice')) 
BEGIN
	ALTER TABLE dbo.SitePrice ADD
		CompetitorId int NULL
END

-- create Markup column
IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'Markup' AND Object_ID = Object_ID(N'SitePrice')) 
BEGIN
	ALTER TABLE dbo.SitePrice ADD
		Markup int NOT NULL CONSTRAINT DF_SitePrice_Markup DEFAULT 0
END

IF EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.[FK_dbo.SitePrice_CompetitorId_dbo.Site_SiteId]')
   AND parent_object_id = OBJECT_ID(N'dbo.SitePrice')
)
BEGIN
	ALTER TABLE [dbo].[SitePrice] DROP CONSTRAINT [FK_dbo.SitePrice_CompetitorId_dbo.Site_SiteId]
END

ALTER TABLE [dbo].[SitePrice]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SitePrice_CompetitorId_dbo.Site_SiteId] FOREIGN KEY([CompetitorId])
REFERENCES [dbo].[Site] ([Id])

ALTER TABLE [dbo].[SitePrice] CHECK CONSTRAINT [FK_dbo.SitePrice_CompetitorId_dbo.Site_SiteId]

