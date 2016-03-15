CREATE TABLE [dbo].[DailyUploadStaging] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [DailyUploadId]     INT NOT NULL,
    [CatNo]             INT NOT NULL,
    [FuelTypeId]        INT NOT NULL,
    [AllStarMerchantNo] INT NOT NULL,
    [ModalPrice]        INT NOT NULL,
    CONSTRAINT [PK_dbo.DailyUploadStaging] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.DailyUploadStaging_dbo.FileUpload_DailyUploadId] FOREIGN KEY ([DailyUploadId]) REFERENCES [dbo].[FileUpload] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_DailyUploadId]
    ON [dbo].[DailyUploadStaging]([DailyUploadId] ASC);

