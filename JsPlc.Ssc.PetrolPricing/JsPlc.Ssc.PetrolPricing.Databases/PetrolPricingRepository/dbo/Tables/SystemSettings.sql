CREATE TABLE [dbo].[SystemSettings]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataCleanseFilesAfterDays] INT NOT NULL DEFAULT (60), 
    [LastDataCleanseFilesOn] DATETIME NULL, 
    [MinUnleadedPrice] INT NOT NULL DEFAULT (500), 
    [MaxUnleadedPrice] INT NOT NULL DEFAULT (4000), 
    [MinDieselPrice] INT NOT NULL DEFAULT (500), 
    [MaxDieselPrice] INT NOT NULL DEFAULT (4000), 
    [MinSuperUnleadedPrice] INT NOT NULL DEFAULT (50), 
    [MaxSuperUnleadedPrice] INT NOT NULL DEFAULT (4000), 
    [MinUnleadedPriceChange] INT NOT NULL DEFAULT (-50), 
    [MaxUnleadedPriceChange] INT NOT NULL DEFAULT (50), 
    [MinDieselPriceChange] INT NOT NULL DEFAULT (-50), 
    [MaxDieselPriceChange] INT NOT NULL DEFAULT (50), 
    [MinSuperUnleadedPriceChange] INT NOT NULL DEFAULT (-50), 
    [MaxSuperUnleadedPriceChange] INT NOT NULL DEFAULT (50), 
    [MaxGrocerDriveTimeMinutes] INT NOT NULL DEFAULT (5), 
    [PriceChangeVarianceThreshold] INT NOT NULL DEFAULT (3), 
    [SuperUnleadedMarkupPrice] INT NOT NULL DEFAULT (50), 
    [DecimalRounding] INT NOT NULL DEFAULT (-1), 
    [EnableSiteEmails] BIT NOT NULL DEFAULT (0), 
    [SiteEmailTestAddresses] VARCHAR(MAX) NOT NULL DEFAULT ('')
)
