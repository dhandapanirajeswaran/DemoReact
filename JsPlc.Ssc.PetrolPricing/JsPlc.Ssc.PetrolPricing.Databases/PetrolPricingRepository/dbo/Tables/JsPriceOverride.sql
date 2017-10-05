CREATE TABLE [dbo].[JsPriceOverride]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UploadId] INT NOT NULL, 
    [CatNo] INT NOT NULL, 
    [UnleadedIncrease] INT NULL, 
    [UnleadedAbsolute] INT NULL, 
    [DieselIncrease] INT NULL, 
    [DieselAbsolute] INT NULL, 
    [SuperUnleadedIncrease] INT NULL, 
    [SuperUnleadedAbsolute] INT NULL
)
