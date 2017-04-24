CREATE TABLE [dbo].[Site] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [CatNo]                  INT            NULL,
    [Brand]                  NVARCHAR (MAX) NULL,
    [SiteName]               NVARCHAR (MAX) NULL,
    [Address]                NVARCHAR (MAX) NULL,
    [Suburb]                 NVARCHAR (MAX) NULL,
    [Town]                   NVARCHAR (MAX) NULL,
    [PostCode]               NVARCHAR (MAX) NULL,
    [Company]                NVARCHAR (MAX) NULL,
    [Ownership]              NVARCHAR (MAX) NULL,
    [StoreNo]                INT            NULL,
    [PfsNo]                  INT            NULL,
    [IsSainsburysSite]       BIT            NOT NULL,
    [IsActive]               BIT            NOT NULL,
    [TrailPriceCompetitorId] INT            NULL,
    [CompetitorPriceOffset] FLOAT NOT NULL DEFAULT (0), 
	[CompetitorPriceOffsetNew] FLOAT NOT NULL DEFAULT (0), 
    [Notes] VARCHAR(MAX) NULL, 
    [PriceMatchType] INT NULL DEFAULT 1, 
    CONSTRAINT [PK_dbo.Site] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Site_TrailPriceCompetitorId_dbo.Site_SiteId] FOREIGN KEY ([TrailPriceCompetitorId]) REFERENCES [dbo].[Site] ([Id])
);

