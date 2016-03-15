CREATE TABLE [dbo].[QuarterlyUploadStaging] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [QuarterlyUploadId] INT            NOT NULL,
    [SainsSiteName]     NVARCHAR (MAX) NULL,
    [SainsSiteTown]     NVARCHAR (MAX) NULL,
    [SainsSiteCatNo]    INT            NOT NULL,
    [Rank]              INT            NOT NULL,
    [DriveDist]         REAL           NOT NULL,
    [DriveTime]         REAL           NOT NULL,
    [CatNo]             INT            NOT NULL,
    [Brand]             NVARCHAR (MAX) NULL,
    [SiteName]          NVARCHAR (MAX) NULL,
    [Addr]              NVARCHAR (MAX) NULL,
    [Suburb]            NVARCHAR (MAX) NULL,
    [Town]              NVARCHAR (MAX) NULL,
    [PostCode]          NVARCHAR (MAX) NULL,
    [Company]           NVARCHAR (MAX) NULL,
    [Ownership]         NVARCHAR (MAX) NULL,
    [AddSiteRow]        BIT            NOT NULL,
    [AddSiteToCompRow]  BIT            NOT NULL,
    CONSTRAINT [PK_dbo.QuarterlyUploadStaging] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.QuarterlyUploadStaging_dbo.FileUpload_QuarterlyUploadId] FOREIGN KEY ([QuarterlyUploadId]) REFERENCES [dbo].[FileUpload] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_QuarterlyUploadId]
    ON [dbo].[QuarterlyUploadStaging]([QuarterlyUploadId] ASC);

