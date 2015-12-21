﻿CREATE PROCEDURE [dbo].[spGetCompetitorPrices]
    @siteId int,
	@forDate DateTime,
	@skipRecs int,
    @takeRecs int
AS

--Declare @siteId int = 1
--Declare @forDate DateTime = '2015-12-17'
--Declare @skipRecs int = 0 
--Declare @takeRecs int = 4

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

Declare @phhToday DateTime = null, @phhYestDate DateTime = null
Declare @todayPriceDate DateTime, @yestPriceDate DateTime 

Set @phhToday = (Select Distinct Top 1 UploadDateTime
						from DailyPrice dp, FileUpload fu where dp.DailyUploadId = fu.ID
							AND DateDiff(day, UploadDateTime, @forDate) Between 0 and @lookBackDays -- go 25 days back at most for eff.
						Order By UploadDateTime Desc)

if (@phhToday is null) Set @todayPriceDate = DateAdd(day, -1, @forDate) -- by default go back 1 days
else set @todayPriceDate = @phhToday

Set @phhYestDate = (Select Distinct Top 1 UploadDateTime
						from DailyPrice dp, FileUpload fu where dp.DailyUploadId = fu.ID
							AND UploadDateTime < @todayPriceDate -- look back from @phhtoday
							AND  DateDiff(day, UploadDateTime, @forDate) Between 1 and @lookBackDays -- go 25 days back at most for eff.
						Order By UploadDateTime Desc)

if (@phhYestDate is null) Set @yestPriceDate = DateAdd(day, -2, @forDate) -- by default go back 2 days
else set @yestPriceDate = @phhYestDate

--Select @todayPriceDate, @yestPriceDate

;With sites as
(
	Select Id
	FROM Site s
	Where (@siteId = 0 OR s.Id = @siteId)
			AND s.IsSainsburysSite = 1 AND s.IsActive = 1 
	Order By Id
	Offset @skipRecs ROWS
	Fetch Next @takeRecs ROWS ONLY
) -- select * from sites
,competitors AS -- competitor information with alongside respective JsSiteId (expand SiteToComp with CompInfo)
(
	Select 
		sc.CompetitorId, sc.SiteId as JsSiteId, 
		sc.Distance, sc.DriveTime, sc.Rank,
	
		compInf.CatNo, compInf.SiteName, 
		compInf.Address, compInf.Suburb, compInf.Town,  
		compInf.IsSainsburysSite, compInf.Brand, compInf.Company, compInf.Ownership
	FROM
	SiteToCompetitor sc
		inner join SITE compInf --
			on sc.CompetitorId = compInf.Id 
) -- select * from competitors
,compForSites as -- limit competitors to only selected sites
(
	Select * 
		from competitors c
			inner join sites s 
			On c.JsSiteId = s.Id
) --select * from compForSites
-- IMPORTANT BELOW CTE could result in nulls for FuelInfo if DP table is empty (but each comp is still there)
,compFuels as -- Note: if no dailyPrices, we cannot say what fuels each comp supports (no other source for such info)
(
   Select Distinct 
		c.CompetitorId,
		dp.FuelTypeId, ft.FuelTypeName -- might be null
   from 
		compForSites c 
		Left Join DailyPrice dp
			On c.CatNo = dp.CatNo 
		Left join FuelType ft
			On dp.FuelTypeId = ft.Id
) -- select * from compFuels
,compWithFuels as -- full comp and (possibly null) fuel info
(
	Select 
		comp.CompetitorId, comp.JsSiteId, comp.CatNo, comp.SiteName, comp.Address, comp.Suburb, comp.Town,  
		comp.IsSainsburysSite, comp.Brand, comp.Company, comp.Ownership, comp.DriveTime, comp.Distance, comp.Rank,

		cf.FuelTypeId, cf.FuelTypeName -- might be null
	From 
		compForSites comp
			Inner Join compFuels cf -- since it could be null result(due to blank DP), but we still wanna show competitors
				On comp.CompetitorId = cf.CompetitorId
) -- select * from compWithFuels

-- ## DAILY PRICES from here
,dailyPriceWithUploadDates as -- Daily prices annotated with UploadDates (whole set could be null)
(
	Select 
		dp.AllStarMerchantNo, dp.CatNo as CompCatNo, dp.DailyUploadId, dp.DateOfPrice, dp.ModalPrice,
		dp.FuelTypeId,

		fu.StatusId, fu.StoredFileName, fu.UploadDateTime, fu.UploadedBy, fu.UploadTypeId, 

		ft.FuelTypeName
	from 
		DailyPrice dp, FileUpload fu, FuelType ft
	Where 
		dp.DailyUploadId = fu.Id
		AND dp.FuelTypeId = ft.Id
) -- select * from dailyPriceWithUploadDates
,dailyPricesComp as 
(
	Select 
		cf.CompetitorId, cf.CatNo as CatNo, cf.SiteName, cf.FuelTypeId as FuelId, 

		dudt.* -- all price data for competitors (maybe null)
	FROM 
		compWithFuels cf
		Left Join dailyPriceWithUploadDates dudt -- might be a null set 
			On cf.CatNo = dudt.CompCatNo And cf.FuelTypeId = dudt.FuelTypeId
	Where 1=1
		And dudt.StatusId = 10 -- success files only
) -- Select * from dailyPricesComp
,todaysPrices as
(
	Select * from dailyPricesComp where DateDiff(day, UploadDateTime, @phhToday) = 0
)
,yesterdaysPrices as
(
	Select * from dailyPricesComp where DateDiff(day, UploadDateTime, @phhYestDate) = 0
)
-- ## Build FINAL Result 
,CompetitorsPrices AS
(
	Select cs.CompetitorId as SiteId, cs.JsSiteId, cs.CatNo,
		cs.SiteName, cs.Address, cs.Suburb, cs.Town,
		cs.IsSainsburysSite, cs.Brand, cs.Company, cs.Ownership,

		cs.Distance, cs.DriveTime, cs.Rank,

		cs.FuelTypeId, cs.FuelTypeName, -- could be null as derived from DP file

		todp.UploadDateTime, todp.ModalPrice, -- could be null 

		yestp.UploadDateTime UploadDateTimeYest, yestp.ModalPrice ModalPriceYest -- could be null
				
	From compWithFuels cs
		Left Outer Join todaysPrices todp
			On cs.CompetitorId = todp.CompetitorId
			And todp.FuelTypeId = cs.FuelTypeId
		Left Outer Join yesterdaysPrices yestp
			On cs.CompetitorId = yestp.CompetitorId
			And yestp.FuelTypeId = cs.FuelTypeId
	Where 1=1
		and (todp.UploadDateTime is null OR DateDiff(day, todp.UploadDateTime, @phhToday) = 0)
		and (yestp.UploadDateTime is null OR DateDiff(day, yestp.UploadDateTime, @phhYestDate) = 0)
)
Select *
from CompetitorsPrices
Order By JsSiteId, SiteId, Rank, DriveTime
