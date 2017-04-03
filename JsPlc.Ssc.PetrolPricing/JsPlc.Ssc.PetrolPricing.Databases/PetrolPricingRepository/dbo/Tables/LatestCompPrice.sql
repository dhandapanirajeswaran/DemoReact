CREATE TABLE [dbo].[LatestCompPrice] (
    [Id]                INT      IDENTITY (1, 1) NOT NULL,
    [UploadId]     INT      NULL,
    [CatNo]             INT      NOT NULL,
	[FuelTypeId]        INT      NOT NULL, 
    [ModalPrice]        INT      NOT NULL,
    CONSTRAINT [PK_dbo.LatestCompPrice] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.LatestCompPrice_dbo.FileUpload_DailyUploadId] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[FileUpload] ([Id]),
    CONSTRAINT [FK_dbo.LatestCompPrice_dbo.FuelType_FuelTypeId] FOREIGN KEY ([FuelTypeId]) REFERENCES [dbo].[FuelType] ([Id])
);



 
GO

CREATE INDEX [IDX_LatestCompPrice_UploadId] ON [dbo].[LatestCompPrice] (UploadId)
