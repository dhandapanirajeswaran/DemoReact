CREATE FUNCTION [dbo].[fn_DoesNearbyGrocerPriceExistForSite]
(
	@ForDate DATE,
	@SiteId INT,
	@DriveTime INT
)
RETURNS INT
AS
BEGIN
	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate)

	--
	-- Check if ANY competitor grocer (SAINSBURYS, ASDA, TESCO, MORRISONS) price exists with X minutes DriveTime
	--

	DECLARE @GrocerPriceExists BIT = CASE WHEN EXISTS(
		SELECT TOP 1 NULL
		FROM
			dbo.SiteToCompetitor stc
			INNER JOIN dbo.Site st on st.Id = stc.CompetitorId AND st.Brand IN ('SAINSBURYS', 'ASDA', 'TESCO', 'MORRISONS')
			INNER JOIN dbo.LatestCompPrice lcp ON lcp.CatNo = st.CatNo
			INNER JOIN dbo.FileUpload fu ON fu.Id = lcp.UploadId AND fu.UploadDateTime >= @ForDate AND fu.UploadDateTime < @ForDateNextDay
		WHERE
			stc.SiteId = @SiteId
			AND stc.DriveTime < @DriveTime
			) THEN 1
			ELSE 0
		END

	RETURN @GrocerPriceExists
END
