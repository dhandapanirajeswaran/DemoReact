CREATE PROCEDURE dbo.spGetAllFuelPriceSettings
AS
BEGIN
	SET NOCOUNT ON;

DECLARE @FuelType_SuperUnleaded INT = 1
DECLARE @FuelType_Unleaded INT = 2
DECLARE @FuelType_Diesel INT = 6

	DECLARE @FuelSettingsTV TABLE (FuelTypeId INT, Markup INT, MinPrice INT, MaxPrice INT, MinPriceChange INT, MaxPriceChange INT)

	-- SUPER UNLEADED --
	INSERT INTO @FuelSettingsTV
	SELECT TOP 1 
		@FuelType_SuperUnleaded,
		ss.SuperUnleadedMarkupPrice, 
		ss.MinSuperUnleadedPrice,
		ss.MaxSuperUnleadedPrice,
		ss.MinSuperUnleadedPriceChange,
		ss.MaxSuperUnleadedPriceChange
	FROM 
		dbo.SystemSettings ss

	-- UNLEADED --
	INSERT INTO @FuelSettingsTV
	SELECT TOP 1
		@FuelType_Unleaded,
		0,
		ss.MinUnleadedPrice,
		ss.MaxUnleadedPrice,
		ss.MinUnleadedPriceChange,
		ss.MaxUnleadedPriceChange
	FROM
		dbo.SystemSettings ss
			
	-- DIESEL --
	INSERT INTO @FuelSettingsTV
	SELECT TOP 1
		@FuelType_Diesel,
		0,
		ss.MinDieselPrice,
		ss.MaxDieselPrice,
		ss.MinDieselPriceChange,
		ss.MaxDieselPriceChange
	FROM
		dbo.SystemSettings ss


	-- Resultset
	SELECT * FROM @FuelSettingsTV
END

