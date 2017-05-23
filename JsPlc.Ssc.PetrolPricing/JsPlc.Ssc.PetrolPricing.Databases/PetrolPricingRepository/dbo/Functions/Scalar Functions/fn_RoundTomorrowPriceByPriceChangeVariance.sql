CREATE FUNCTION [dbo].[fn_RoundTomorrowPriceByPriceChangeVariance]
(
	@PriceChangeVarianceThreshold INT,
	@TodayPrice INT,
	@TomorrowPrice INT
)
RETURNS INT
AS
BEGIN
	DECLARE @RoundedTomorrowPrice INT = @TomorrowPrice

	IF @TodayPrice > 0 AND @TomorrowPrice > 0
	BEGIN
		DECLARE @Diff INT = ABS(@TomorrowPrice - @TodayPrice)
		IF @Diff <= @PriceChangeVarianceThreshold
			SET @RoundedTomorrowPrice = @TodayPrice
	END

	RETURN @RoundedTomorrowPrice
END
