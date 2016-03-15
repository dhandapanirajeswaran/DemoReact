CREATE TABLE [dbo].[ImportProcessError] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [UploadId]        INT            NOT NULL,
    [RowOrLineNumber] INT            NOT NULL,
    [ErrorMessage]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ImportProcessError] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ImportProcessError_dbo.FileUpload_UploadId] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[FileUpload] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UploadId]
    ON [dbo].[ImportProcessError]([UploadId] ASC);

