CREATE PROCEDURE [dbo].[spGetLastSitePricingDate]
AS
BEGIN
	SET NOCOUNT ON

	SELECT MAX(DateOfCalc) [LastSitePricingDate] FROM dbo.SitePrice
END
	
