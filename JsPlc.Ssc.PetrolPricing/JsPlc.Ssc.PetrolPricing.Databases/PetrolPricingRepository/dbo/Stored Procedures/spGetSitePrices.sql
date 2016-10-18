﻿CREATE PROCEDURE [dbo].[spGetSitePrices]
	@siteId int,
	@forDate DateTime,
	@skipRecs int,
	@takeRecs int,
	@storeName nvarchar(500),
	@storeTown nvarchar(500),
	@catNo int,
	@storeNo int
AS

--Declare @siteId int = 0
--Declare @forDate DateTime = '2015-12-18'
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
set @todayPriceDate =@forDate
--Select @todayPriceDate

;With sites as
(
	Select *
	FROM Site s
	Where (@siteId = 0 OR s.Id = @siteId)
			AND (@catNo = 0 OR s.CatNo = @catNo)
			AND (@storeNo = 0 OR s.StoreNo = @storeNo)
			AND (@storeName = '' OR s.SiteName like ('%' + @storeName + '%'))
			AND (@storeTown = '' OR s.Town like ('%' + @storeTown + '%'))
			AND s.IsSainsburysSite = 1 
			AND s.IsActive = 1 
	Order By Id
	Offset @skipRecs ROWS
	Fetch Next @takeRecs ROWS ONLY
) -- select * from sites
,siteFuels as
(
   Select Distinct s.Id as SiteId, dp.FuelTypeId,dp.ModalPrice, ft.FuelTypeName, s.CompetitorPriceOffset
   from sites s, DailyPrice dp, FuelType ft
   Where s.CatNo = dp.CatNo and dp.FuelTypeId = ft.Id
   Union 
   Select Distinct s.Id as SiteId, sp.FuelTypeId,sp.SuggestedPrice, ft.FuelTypeName, s.CompetitorPriceOffset
   from sites s, SitePrice sp, FuelType ft
   Where s.Id = sp.SiteId and sp.FuelTypeId = ft.Id
) -- select * from siteFuels
,sitesWithFuels as
(
	Select distinct s.Id as SiteId, s.CatNo, s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,
		sf.FuelTypeId, sf.FuelTypeName,sf.ModalPrice, s.PfsNo, s.StoreNo
	From 
		[Site] s 
			Inner Join siteFuels sf
				On s.Id = sf.SiteId
) -- select * from sitesWithFuels
,sitePrices as
(
	Select 
		distinct sf.SiteId, sf.FuelTypeId, sf.FuelTypeName, 

		sp.DateOfCalc, sp.DateOfPrice, sp.EffDate,
		sp.SuggestedPrice, sp.OverriddenPrice, sp.CompetitorId, sp.Markup, sp.IsTrailPrice,
		sf.CompetitorPriceOffset, sf.ModalPrice
	FROM siteFuels sf Left Join SitePrice sp
		On sf.FuelTypeId = sp.FuelTypeId And sf.SiteId = sp.SiteId
) -- Select * from sitePrices
,tomorrowsPrices as
(
	Select distinct * from sitePrices Where DateDiff(day, DateOfPrice, @forDate) = 0
) -- Select * from tomorrowsPrices
,todaysPrices as -- treat lastPriceDate as todaysPrice
(
	Select distinct * from sitePrices Where DateDiff(day, DateOfPrice, @todayPriceDate) = 0
) -- Select * from todaysPrices
,sitesWithPrices As -- JS Site and Prices information
(
	SELECT  distinct swf.SiteId, swf.CatNo,
		swf.SiteName, swf.Address, swf.Suburb, swf.Town,  
		swf.IsSainsburysSite, swf.Brand, swf.Company, swf.Ownership,
		swf.FuelTypeId, swf.FuelTypeName, swf.PfsNo, swf.StoreNo,

		tomp.DateOfCalc, tomp.DateOfPrice, 
		tomp.SuggestedPrice, tomp.OverriddenPrice,
		tomp.CompetitorId, tomp.Markup, tomp.IsTrailPrice,

		todp.DateOfCalc DateOfCalcForTodaysPrice, todp.DateOfPrice DateOfPriceForTodaysPrice, 
		todp.ModalPrice SuggestedPriceToday, todp.OverriddenPrice OverriddenPriceToday,
		todp.CompetitorPriceOffset
	FROM 
	sitesWithFuels swf
		Left join tomorrowsPrices as tomp
			On swf.SiteId = tomp.SiteId
			And tomp.FuelTypeId = swf.FuelTypeId
		Left join todaysPrices as todp
			On swf.SiteId = todp.SiteId
			And todp.FuelTypeId = swf.FuelTypeId
	Where 
		(tomp.DateOfPrice is null OR DateDiff(day, tomp.DateOfPrice, @forDate) = 0)
		AND (todp.DateOfPrice is null OR DateDiff(day, todp.DateOfPrice, @todayPriceDate) = 0)
)
Select distinct * from sitesWithPrices 
Order By SiteName