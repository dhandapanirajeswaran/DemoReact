CREATE TABLE [dbo].[PriceSnapshotRow]
(
	[PriceSnapshotId] [int] NOT NULL,
	[SiteId] [int] NULL,
	[FuelTypeId] [int] NULL,
	[AutoPrice] [int] NULL,
	[OverridePrice] [int] NULL,
	[TodayPrice] [int] NULL,
	[Markup] [float] NULL,
	[CompetitorName] [nvarchar](max) NULL,
	[IsTrailPrice] [int] NULL,
	[CompetitorPriceOffset] [float] NULL,
	[PriceMatchType] [int] NULL,
	[PriceSource] [varchar](19) NULL,
	[PriceSourceDateTime] [datetime] NULL,
	[CompetitorSiteId] [int] NULL,
	[Distance] [real] NULL,
	[DriveTime] [real] NULL,
	[DriveTimePence] [int] NULL 
)
