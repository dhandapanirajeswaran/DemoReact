CREATE TABLE [dbo].[PriceReasonFlags]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [BitMask] INT NOT NULL, 
    [Descript] VARCHAR(100) NOT NULL
)
