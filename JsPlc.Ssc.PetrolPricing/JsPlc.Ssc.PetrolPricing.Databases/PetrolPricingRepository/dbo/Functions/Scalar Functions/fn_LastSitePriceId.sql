CREATE FUNCTION dbo.fn_LastSitePriceId
(
	@SiteId INT,
	@FuelTypeId INT,
	@ForDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @SitePriceId INT

	SELECT TOP 1
		@SitePriceId = sp.Id
	FROM
		dbo.SitePrice sp
	WHERE
		sp.SiteId = @SiteId
		AND
		sp.FuelTypeId = @FuelTypeId
		AND
		sp.DateOfCalc = (SELECT MAX(DateOfCalc) FROM dbo.SitePrice WHERE SiteId = @SiteId AND FuelTypeId = @FuelTypeId AND SuggestedPrice > 0 AND DateOfCalc <= @ForDate)
	
	RETURN @SitePriceId
END
GO

