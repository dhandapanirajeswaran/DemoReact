CREATE TABLE [dbo].[EmailTemplate]
(
	[EmailTemplateId] [int] IDENTITY(1,1) NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[TemplateName] [nvarchar](100) NOT NULL,
	[SubjectLine] [nvarchar](200) NOT NULL,
	[PPUserId] [int] NOT NULL,
	[EmailBody] [nvarchar](max) NOT NULL, 
    CONSTRAINT [PK_EmailTemplate] PRIMARY KEY ([EmailTemplateId])
)
