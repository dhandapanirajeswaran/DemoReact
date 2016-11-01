CREATE TABLE [dbo].[LatestPrice] (
    [Id]                INT      IDENTITY (1, 1) NOT NULL,
    [UploadId]     INT      NULL,
    [PfsNo]             INT      NOT NULL,
	[StoreNo]             INT      NOT NULL,
    [FuelTypeId]        INT      NOT NULL, 
    [ModalPrice]        INT      NOT NULL,
    CONSTRAINT [PK_dbo.LatestPrice] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.LatestPrice_dbo.FileUpload_DailyUploadId] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[FileUpload] ([Id]),
    CONSTRAINT [FK_dbo.LatestPrice_dbo.FuelType_FuelTypeId] FOREIGN KEY ([FuelTypeId]) REFERENCES [dbo].[FuelType] ([Id])
);



 