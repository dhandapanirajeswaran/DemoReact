CREATE FUNCTION dbo.fn_TodayPriceForSiteFuelDate
(
	@SiteId INT,
	@FuelTypeId INT,
	@ForDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @TodayPrice INT

	DECLARE @IsSainsburysSite BIT = (SELECT TOP 1 IsSainsburysSite FROM dbo.Site WHERE Id = @SiteId)

	IF @IsSainsburysSite = 1
	BEGIN
		-- Sainsburys price = dbo.SitePrice table
		SELECT TOP 1
			@TodayPrice = CASE
				WHEN sp.OverriddenPrice > 0 THEN sp.OverriddenPrice
				WHEN sp.SuggestedPrice > 0 THEN sp.SuggestedPrice
				ELSE 0
			END
		FROM
			dbo.SitePrice sp
		WHERE
			sp.Id = (
				SELECT TOP 1
					Id
				FROM
					dbo.SitePrice
				WHERE
					SiteId = @SiteId
					AND
					FuelTypeId = @FuelTypeId
					AND
					DateOfCalc < @ForDate -- NOTE: DateOfCalc are done on the previous day
				ORDER BY
					DateOfCalc DESC
			)
	END
	ELSE
	BEGIN
		-- Non-Sainsburys price = dbo.CompetitorPrice table
		SELECT TOP 1
			@TodayPrice = cp.ModalPrice
		FROM
			dbo.CompetitorPrice cp
		WHERE
			cp.SiteId = @SiteId
			AND
			cp.FuelTypeId = @FuelTypeId
			AND
			cp.DateOfPrice = (SELECT MAX(DateOfPrice) FROM dbo.CompetitorPrice WHERE SiteId = @SiteId AND FuelTypeId = @FuelTypeId AND DateOfPrice <= @ForDate)
	END

	RETURN COALESCE(@TodayPrice, 0)
END

