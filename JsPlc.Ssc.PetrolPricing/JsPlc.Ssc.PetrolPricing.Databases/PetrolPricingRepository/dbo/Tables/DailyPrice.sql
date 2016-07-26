CREATE TABLE [dbo].[DailyPrice] (
    [Id]                INT      IDENTITY (1, 1) NOT NULL,
    [DailyUploadId]     INT      NULL,
    [CatNo]             INT      NOT NULL,
    [FuelTypeId]        INT      NOT NULL,
    [AllStarMerchantNo] INT      NOT NULL,
    [DateOfPrice]       DATETIME NOT NULL,
    [ModalPrice]        INT      NOT NULL,
    CONSTRAINT [PK_dbo.DailyPrice] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.DailyPrice_dbo.FileUpload_DailyUploadId] FOREIGN KEY ([DailyUploadId]) REFERENCES [dbo].[FileUpload] ([Id]),
    CONSTRAINT [FK_dbo.DailyPrice_dbo.FuelType_FuelTypeId] FOREIGN KEY ([FuelTypeId]) REFERENCES [dbo].[FuelType] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_DailyUploadId]
    ON [dbo].[DailyPrice]([DailyUploadId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FuelTypeId]
    ON [dbo].[DailyPrice]([FuelTypeId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_CatNo_FuleTypeId_DailyUploadId] ON [dbo].[DailyPrice]
(
	[CatNo] ASC,
	[FuelTypeId] ASC,
	[DailyUploadId] ASC
)
INCLUDE ( 	[ModalPrice]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
