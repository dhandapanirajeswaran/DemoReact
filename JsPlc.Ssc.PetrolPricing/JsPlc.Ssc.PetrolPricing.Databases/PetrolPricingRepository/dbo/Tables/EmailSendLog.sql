CREATE TABLE [dbo].[EmailSendLog] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [SiteId]              INT             NOT NULL,
    [IsTest]              BIT             NOT NULL,
    [EmailFrom]           NVARCHAR (500)  NULL,
    [FixedEmailTo]        NVARCHAR (500)  NULL,
    [ListOfEmailTo]       NVARCHAR (1500) NULL,
    [EmailSubject]        NVARCHAR (1500) NULL,
    [EmailBody]           NVARCHAR (MAX)  NULL,
    [EndTradeDate]        DATETIME        NOT NULL,
    [SendDate]            DATETIME        NOT NULL,
    [LoginUser]           NVARCHAR (500)  NULL,
    [IsSuccess]           BIT             NOT NULL,
    [IsWarning]           BIT             NOT NULL,
    [IsError]             BIT             NOT NULL,
    [CommaSeprSiteCatIds] NVARCHAR (MAX)  NULL,
    [WarningMessage]      NVARCHAR (MAX)  NULL,
    [ErrorMessage]        NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_dbo.EmailSendLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

