CREATE TABLE [dbo].[DriveTimeMarkup]
(
	[Id] INT NOT NULL IDENTITY, 
    [FuelTypeId] INT NOT NULL, 
    [DriveTime] INT NOT NULL, 
    [Markup] INT NULL, 
    CONSTRAINT [PK_DriveTimeMarkup] PRIMARY KEY ([Id]) 
)

GO

CREATE INDEX [IX_DriveTimeMarkup_Column] ON [dbo].[DriveTimeMarkup] ([FuelTypeId], [DriveTime])
