﻿CREATE PROCEDURE [dbo].[spUpsertPriceFreezeEvent]
	@PriceFreezeEventId INT,
    @DateFrom DATE, 
    @DateTo DATE,
    @IsActive BIT,
    @CreatedBy VARCHAR(200),
	@FuelTypeId INT
AS
BEGIN
	SET NOCOUNT ON

----DEBUG:START
--DECLARE	@PriceFreezeEventId INT = 2
--DECLARE   @DateFrom DATE = '2017-09-01 00:00:00.000'
--DECLARE   @DateTo DATE = '2017-09-30 00:00:00.000'
--DECLARE   @IsActive BIT = 1
--DECLARE   @CreatedBy VARCHAR(200) = 'garry.leeder@sainsburys.co.uk'
--DECLARE	@FuelTypeId INT = 2
----DEBUG:END

	DECLARE @ErrorCode INT = 0

	IF @DateTo < @DateFrom
	BEGIN
		SET @ErrorCode = -1
	END

	--
	-- Check for overlapping Date Ranges for a FuelType
	--
	IF @ErrorCode = 0 AND
		EXISTS(SELECT TOP 1 NULL FROM dbo.PriceFreezeEvent WHERE PriceFreezeEventId <> @PriceFreezeEventId AND FuelTypeId = @FuelTypeId AND dbo.fn_IsDateOverlap(DateFrom, DateTo, @DateFrom, @DateTo) = 1)
	BEGIN
		SET @ErrorCode = -2
	END

	IF @ErrorCode = 0 
		BEGIN
		IF @PriceFreezeEventId = 0
		BEGIN
			INSERT INTO dbo.PriceFreezeEvent
			(
				[DateFrom],
				[DateTo],
				[CreatedOn],
				[CreatedBy],
				[IsActive],
				[FuelTypeId]
			)
			VALUES (
				@DateFrom,
				@DateTo,
				GETDATE(),
				@CreatedBy,
				@IsActive,
				@FuelTypeId
			);
			SET @PriceFreezeEventId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			UPDATE dbo.PriceFreezeEvent
			SET
				DateFrom = @DateFrom,
				DateTo = @DateTo,
				IsActive = @IsActive,
				CreatedBy = @CreatedBy,
				FuelTypeId = @FuelTypeId
			WHERE
				PriceFreezeEventId = @PriceFreezeEventId;
		END
	END

	SELECT @ErrorCode [ErrorCode]
END
