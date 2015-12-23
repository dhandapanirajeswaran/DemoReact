CREATE PROCEDURE [dbo].[spSetSuperUnleadedPricesFromUnleaded]
	@siteId int,
	@forDate DateTime,
	@SuperUnleadedMarkup int
AS

--Declare @siteId int = 0 -- Uncomment for manual testing
--Declare @forDate DateTime = '2015-12-18' -- Uncomment for manual testing
--Declare @SuperUnleadedMarkup Int = 5 -- Uncomment for manual testing
If(@SuperUnleadedMarkup = 0) Set @SuperUnleadedMarkup = 5

Insert Into SitePrice (SiteId, FuelTypeId, DateOfCalc, DateOfPrice, UploadId, EffDate, SuggestedPrice, OverriddenPrice) -- comment out for manual test
Select SiteId, 1, DateOfCalc, DateOfPrice, UploadId, EffDate, SuggestedPrice + @SuperUnleadedMarkup, 0 from -- comment out for manual test
-- Select * from -- Uncomment for manual testing
(
	Select 
		s1.*, s2.ID as S2Id, s2.FuelTypeId as S2FuelTypeId
	from 
		(Select * from SitePrice sp 
			Where FuelTypeId = 2 
			AND (@siteId = 0 Or sp.SiteId = @siteId)) s1 -- Unleaded
		Left join (Select * from SitePrice Where FuelTypeId = 1) s2 -- SuperUnleaded
		On s1.SiteId = s2.SiteId And s1.UploadId = s2.UploadId
	Where 1=1
		AND DateDiff(Day, s1.DateOfCalc, @forDate) = 0 -- for a given date
) s3
Where s3.S2Id is null
