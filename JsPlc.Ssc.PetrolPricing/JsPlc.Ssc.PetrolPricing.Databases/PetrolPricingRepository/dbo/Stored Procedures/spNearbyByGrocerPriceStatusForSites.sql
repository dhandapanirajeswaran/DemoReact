CREATE PROCEDURE [dbo].[spNearbyByGrocerPriceStatusForSites]
	@ForDate DATE,
	@DriveTime INT,
	@SiteIds VARCHAR(MAX)
AS
	SET NOCOUNT ON;

	SELECT 
		st.Id [SiteId],
		dbo.fn_DoesNearbyGrocerPriceExistForSite(@ForDate, st.Id, @DriveTime) [HasNearbyGrocerPrice]
	FROM 
		dbo.tf_SplitIdsOnComma(@SiteIds) st

RETURN 0
