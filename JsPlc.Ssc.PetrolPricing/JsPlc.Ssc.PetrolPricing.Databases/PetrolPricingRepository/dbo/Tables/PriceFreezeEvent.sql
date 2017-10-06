CREATE TABLE [dbo].[PriceFreezeEvent]
(
	[PriceFreezeEventId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DateFrom] DATETIME NOT NULL, 
    [DateTo] DATETIME NOT NULL, 
    [CreatedOn] DATETIME NOT NULL, 
    [CreatedBy] VARCHAR(200) NOT NULL, 
    [IsActive] BIT NOT NULL, 
    [FuelTypeId] INT NOT NULL DEFAULT (2)
)
