CREATE PROCEDURE [dbo].[spDeletePriceFreezeEvent]
	@PriceFreezeEventId INT
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM dbo.PriceFreezeEvent WHERE PriceFreezeEventId = @PriceFreezeEventId;

	RETURN 0
END
