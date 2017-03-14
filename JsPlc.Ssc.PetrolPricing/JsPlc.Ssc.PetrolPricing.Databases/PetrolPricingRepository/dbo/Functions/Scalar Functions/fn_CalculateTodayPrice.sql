-- =============================================
--      Author: Garry Leeder
--     Created: 2017-03-14
--    Modified: 2017-03-14
-- Description:	Calculate 'Todays' price
-- =============================================
CREATE FUNCTION dbo.fn_CalculateTodayPrice
(
	@fileUpload_UploadDateTime DATE,
	@latestPrice_Id INT,
	@latestPrice_ModalPrice INT,
	@overridepriceIfAny_Id INT,
	@overridepriceIfAny_DateOfPrice DATE,
	@overridepriceIfAny_OverriddenPrice INT,
	@todayPriceSortByDate_Id INT,
	@todayPriceSortByDate_DateOfPrice DATE,
	@todayPriceSortByDate_ModalPrice INT
)
RETURNS INT
AS
BEGIN

    IF @todayPriceSortByDate_Id IS NULL AND @overridepriceIfAny_Id IS NULL AND @latestPrice_Id IS NOT NULL
    BEGIN
        RETURN @latestPrice_ModalPrice
    END

	IF @todayPriceSortByDate_Id IS NULL AND @overridepriceIfAny_Id IS NOT NULL AND @latestPrice_Id IS NULL
    BEGIN
        RETURN @overridepriceIfAny_OverriddenPrice
    END
			
	IF @todayPriceSortByDate_Id IS NOT NULL AND @overridepriceIfAny_Id IS NULL AND @latestPrice_Id IS NULL
    BEGIN
        RETURN @todayPriceSortByDate_ModalPrice
    END
			
    IF @todayPriceSortByDate_Id IS NOT NULL AND @overridepriceIfAny_Id IS NOT NULL AND @latestPrice_Id IS NULL
    BEGIN
        RETURN CASE WHEN @todayPriceSortByDate_DateOfPrice > @overridepriceIfAny_DateOfPrice
            THEN @todayPriceSortByDate_ModalPrice
            ELSE @overridepriceIfAny_OverriddenPrice
		END
    END
			
    IF @todayPriceSortByDate_Id IS NULL AND @overridepriceIfAny_Id IS NOT NULL AND @latestPrice_Id IS NOT NULL
    BEGIN
        RETURN CASE WHEN @fileUpload_UploadDateTime > @overridepriceIfAny_DateOfPrice
            THEN @latestPrice_ModalPrice
            ELSE @overridepriceIfAny_OverriddenPrice
		END
    END
			
    IF @todayPriceSortByDate_Id IS NOT NULL AND @overridepriceIfAny_Id IS NULL AND @latestPrice_Id IS NOT NULL
    BEGIN
        RETURN CASE WHEN @todayPriceSortByDate_DateOfPrice > @fileUpload_UploadDateTime
            THEN @todayPriceSortByDate_ModalPrice
            ELSE @latestPrice_ModalPrice
		END
    END
			
    IF @todayPriceSortByDate_Id IS NOT NULL AND @overridepriceIfAny_Id IS NOT NULL AND @latestPrice_Id IS NOT NULL
    BEGIN
        IF @todayPriceSortByDate_DateOfPrice > @fileUpload_UploadDateTime
        BEGIN
            IF @todayPriceSortByDate_DateOfPrice > @overridepriceIfAny_DateOfPrice
            BEGIN
                RETURN @todayPriceSortByDate_ModalPrice
            END
					
            RETURN CASE WHEN @fileUpload_UploadDateTime > @overridepriceIfAny_DateOfPrice
				THEN @latestPrice_ModalPrice
				ELSE @overridepriceIfAny_OverriddenPrice
            END
        END
        else
        BEGIN
            RETURN CASE WHEN @fileUpload_UploadDateTime > @overridepriceIfAny_DateOfPrice
				THEN @latestPrice_ModalPrice
				ELSE @overridepriceIfAny_OverriddenPrice
            END
        END
    END

	RETURN 0
END
