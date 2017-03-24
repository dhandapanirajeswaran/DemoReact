CREATE TABLE [dbo].[ContactDetails]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Heading] NVARCHAR(400) NULL,
    [Address] NVARCHAR(400) NULL, 
    [PhoneNumber] VARCHAR(100) NULL, 
    [EmailName] VARCHAR(100) NULL, 
    [EmailAddress] VARCHAR(100) NULL,
	[IsActive] BIT DEFAULT(1)

)
