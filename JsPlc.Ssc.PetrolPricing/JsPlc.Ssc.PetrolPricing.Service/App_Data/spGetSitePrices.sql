CREATE PROCEDURE dbo.spGetSitePrices
    @siteId int,
	@forDate DateTime,
	@skipRecs int,
	@takeRecs int
AS

--Declare @siteId int = 0
--Declare @forDate DateTime = '2015-11-30'
--Declare @skipRecs int = 1
--Declare @takeRecs int = 2

/* Testbed for Day Of Week, DateAdd calc
Declare @thedate DateTime = '2015-12-08' -- Tue 
Select ( @@datefirst - 1 + datepart(weekday, @thedate) ) % 7 -- Sunday always 0, Tue = 2, Fri = 5, Sat = 6
Select DateAdd(day, -2, @theDate) -- to do back 2 days (gives 2015-12-06)
Declare @anotherDt DateTime = '2015-12-09'
Select  DateDiff(day, @anotherDt, @thedate) -- (gives 2nd param - 1st param)
*/

-- DailyPrice UploadDateTime = 08 Dec 2015 (means prices are actually for the Day before i.e. 07 Dec 2015)
-- SitePrices DateOfCalc = 08 Dec 2015 (means prices are actually for the Day after i.e. 09 Dec 2015)

-- If today is 08 Dec 2015:
-- And if we want Competitor Prices of yesterday, we look at DailyPrice UploadDateTime = 08 Dec 2015

;With sitePrices as
(
	Select sp.siteId, sp.FuelTypeId, sp.DateOfCalc, sp.DateOfPrice, sp.EffDate, 
	sp.SuggestedPrice, sp.OverriddenPrice, ft.FuelTypeName
	FROM SitePrice sp, FuelType ft
	Where sp.FuelTypeId = ft.Id
)
,sites as
(
	Select *
	FROM Site s
	Where (@siteId = 0 OR s.Id = @siteId)
	Order By Id
	Offset @skipRecs ROWS
	Fetch Next @takeRecs ROWS ONLY
)
,todaysPrices as
(
	Select * from sitePrices Where DateDiff(day, DateOfCalc, @forDate) = 0
)
,yesterdaysPrices as
(
	Select * from sitePrices Where DateDiff(day, DateOfCalc, @forDate) = 1
)
,sitesWithPrices As -- JS Site and Prices information
(
    SELECT s.Id as siteId, s.CatNo, 
		s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,

		todaysPrices.FuelTypeId, todaysPrices.FuelTypeName, 
		todaysPrices.DateOfCalc, todaysPrices.DateOfPrice, 
		todaysPrices.SuggestedPrice, todaysPrices.OverriddenPrice,

		yesterdaysPrices.DateOfCalc DateOfCalcYest, yesterdaysPrices.DateOfPrice DateOfPriceYest, 
		yesterdaysPrices.SuggestedPrice SuggestedPriceYest, yesterdaysPrices.OverriddenPrice OverriddenPriceYest
	FROM 
	Sites s 
		Left outer join todaysPrices
			On s.Id = todaysPrices.SiteId
		Left outer join yesterdaysPrices
			On s.Id = yesterdaysPrices.SiteId
	Where 
		(todaysPrices.DateOfCalc is null OR DateDiff(day, todaysPrices.DateOfCalc, @forDate) = 0)
		AND (yesterdaysPrices.DateOfCalc is null OR DateDiff(day, yesterdaysPrices.DateOfCalc, @forDate) = 1)
		AND s.IsSainsburysSite = 1
)
Select * from sitesWithPrices 
Order By SiteId
