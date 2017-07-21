CREATE TABLE [dbo].[PriceSnapshot]
(
	[PriceSnapshotId] [int] NOT NULL IDENTITY,
	[DateFrom] [date] NOT NULL,
	[DateTo] [date] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsOutdated] [bit] NOT NULL, 
    [IsRecalcRequired] BIT NOT NULL DEFAULT (0), 
    CONSTRAINT [PK_PriceSnapshot] PRIMARY KEY ([PriceSnapshotId]),
)
