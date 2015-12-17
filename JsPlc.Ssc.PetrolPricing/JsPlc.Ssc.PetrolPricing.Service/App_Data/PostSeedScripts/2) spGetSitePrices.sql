CREATE PROCEDURE [dbo].[spGetSitePrices]
	@siteId int,
	@forDate DateTime,
	@skipRecs int,
	@takeRecs int
AS

--Declare @siteId int = 0
--Declare @forDate DateTime = '2015-12-16'
--Declare @skipRecs int = 0
--Declare @takeRecs int = 20

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
Declare @lookBackDays int = 25

Declare @todayPriceDate DateTime, @lastPriceDate DateTime = null
Set @lastPriceDate = (Select Distinct Top 1 DateOfCalc
						from SitePrice 
						where DateDiff(day, DateOfCalc, @forDate) Between 1 and @lookBackDays -- go 25 days back at most for eff.
						Order By DateOfCalc Desc)

if (@lastPriceDate is null) Set @todayPriceDate = DateAdd(day, -1, @forDate)
else set @todayPriceDate = @lastPriceDate
--Select @todayPriceDate

;With sites as
(
	Select *
	FROM Site s
	Where (@siteId = 0 OR s.Id = @siteId)
			AND s.IsSainsburysSite = 1
	Order By Id
	Offset @skipRecs ROWS
	Fetch Next @takeRecs ROWS ONLY
) -- select * from sites
,siteFuels as
(
   Select Distinct s.Id as SiteId, dp.FuelTypeId, ft.FuelTypeName
   from sites s, DailyPrice dp, FuelType ft
   Where s.CatNo = dp.CatNo and dp.FuelTypeId = ft.Id
) -- select * from siteFuels
,sitePrices as
(
	Select 
		sf.SiteId, sf.FuelTypeId, sf.FuelTypeName, 

		sp.DateOfCalc, sp.DateOfPrice, sp.EffDate,
		sp.SuggestedPrice, sp.OverriddenPrice
	FROM siteFuels sf Left Join SitePrice sp
		On sf.FuelTypeId = sp.FuelTypeId And sf.SiteId = sp.SiteId
) -- Select * from sitePrices
,tomorrowsPrices as
(
	Select * from sitePrices Where DateDiff(day, DateOfCalc, @forDate) = 0
) -- Select * from tomorrowsPrices
,todaysPrices as -- treat lastPriceDate as todaysPrice
(
	Select * from sitePrices Where DateDiff(day, DateOfCalc, @todayPriceDate) = 0
) -- Select * from todaysPrices
,sitesWithPrices As -- JS Site and Prices information
(
	SELECT s.Id as siteId, s.CatNo, 
		s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,

		--tomorrowsPrices.FuelTypeId, tomorrowsPrices.FuelTypeName, 
		tomorrowsPrices.DateOfCalc, tomorrowsPrices.DateOfPrice, 
		tomorrowsPrices.SuggestedPrice, tomorrowsPrices.OverriddenPrice,

		todaysPrices.DateOfCalc DateOfCalcForTodaysPrice, todaysPrices.DateOfPrice DateOfPriceForTodaysPrice, 
		todaysPrices.SuggestedPrice SuggestedPriceToday, todaysPrices.OverriddenPrice OverriddenPriceToday
	FROM 
	Sites s
		Left join tomorrowsPrices
			On s.Id = tomorrowsPrices.SiteId
		Left join todaysPrices
			On s.Id = todaysPrices.SiteId
	Where 
		(tomorrowsPrices.DateOfCalc is null OR DateDiff(day, tomorrowsPrices.DateOfCalc, @forDate) = 0)
		AND (todaysPrices.DateOfCalc is null OR DateDiff(day, todaysPrices.DateOfCalc, @todayPriceDate) = 0)
)
Select * from sitesWithPrices 
Order By SiteId

