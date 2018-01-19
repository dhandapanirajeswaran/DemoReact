CREATE TABLE [dbo].[CompetitorPrice]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SiteId] [int] NOT NULL,
	[FuelTypeId] [int] NOT NULL,
	[DateOfPrice] [date] NOT NULL,
	[ModalPrice] [int] NOT NULL,
	[DailyPriceId] [int] NULL,
	[LatestCompPriceId] [int] NULL
)
GO

CREATE UNIQUE INDEX [IX_CompetitorPrice_SiteFuelDate] 
	ON [dbo].[CompetitorPrice] ([SiteId], [FuelTypeId], [DateOfPrice])
GO

CREATE INDEX [missing_index_22_21_CompetitorPrice] 
	ON [dbo].[CompetitorPrice] ([DateOfPrice])

