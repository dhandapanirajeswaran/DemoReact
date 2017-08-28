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