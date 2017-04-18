CREATE TABLE [dbo].[FileUpload] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [OriginalFileName] NVARCHAR (MAX) NOT NULL,
    [StoredFileName]   NVARCHAR (MAX) NOT NULL,
    [UploadTypeId]     INT            NOT NULL,
    [UploadDateTime]   DATETIME       NOT NULL,
    [StatusId]         INT            NOT NULL,
    [UploadedBy]       NVARCHAR (MAX) NOT NULL,
    [FileExists] BIT NOT NULL DEFAULT (0), 
    CONSTRAINT [PK_dbo.FileUpload] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.FileUpload_dbo.ImportProcessStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[ImportProcessStatus] ([Id]),
    CONSTRAINT [FK_dbo.FileUpload_dbo.UploadType_UploadTypeId] FOREIGN KEY ([UploadTypeId]) REFERENCES [dbo].[UploadType] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UploadTypeId]
    ON [dbo].[FileUpload]([UploadTypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StatusId]
    ON [dbo].[FileUpload]([StatusId] ASC);

