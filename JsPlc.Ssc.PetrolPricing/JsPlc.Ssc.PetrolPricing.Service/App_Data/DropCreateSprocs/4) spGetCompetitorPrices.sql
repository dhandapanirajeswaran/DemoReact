CREATE PROCEDURE [dbo].[spGetCompetitorPrices]
    @siteId int,
	@forDate DateTime
AS

--Declare @siteId int = 1
--Declare @forDate DateTime = '2015-12-16'

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
							AND DateDiff(day, UploadDateTime, @forDate) Between 1 and @lookBackDays -- go 25 days back at most for eff.
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

--Select @todayPriceDate

;With competitors AS -- competitor information with alongside respective JsSiteId
(
	Select sc.SiteId as JsSiteId, s.id as CompetitorId, s.CatNo, 
		s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,

		sc.Distance, sc.DriveTime, sc.Rank
	FROM
	SiteToCompetitor sc
		inner join Site s
			On sc.CompetitorId = s.Id
	Where (@siteId = 0 OR s.Id = @siteId)
) --select * from competitors
,compFuels as 
(
   Select Distinct s.CompetitorId, dp.FuelTypeId, ft.FuelTypeName
   from 
		competitors s, DailyPrice dp, FuelType ft
   Where 
		s.CatNo = dp.CatNo and dp.FuelTypeId = ft.Id
) --select * from compFuels
,compWithFuels as
(
	Select 
		comp.CompetitorId, comp.JsSiteId, comp.CatNo, comp.SiteName, comp.Address, comp.Suburb, comp.Town,  
		comp.IsSainsburysSite, comp.Brand, comp.Company, comp.Ownership, comp.DriveTime, comp.Distance, comp.Rank,

		sf.FuelTypeId, sf.FuelTypeName
	From 
		competitors comp 
			Inner Join compFuels sf
				On comp.CompetitorId = sf.CompetitorId
) -- select * from compWithFuels
,dailyPriceWithUploadDates as
(
	Select 
		dp.AllStarMerchantNo, dp.CatNo, dp.DailyUploadId, dp.DateOfPrice, dp.FuelTypeId, dp.ModalPrice, 

		fu.StatusId, fu.StoredFileName, fu.UploadDateTime, fu.UploadedBy, fu.UploadTypeId, 

		ft.FuelTypeName
	from 
		DailyPrice dp, FileUpload fu, FuelType ft
	Where 
		dp.DailyUploadId = fu.Id
		AND dp.FuelTypeId = ft.Id
		--AND fu.UploadTypeId = 1
) -- select * from dailyPriceWithUploadDates
,dailyPricesComp as
(
	Select 
		sf.CompetitorId, sf.FuelTypeId as FuelId, 

		dudt.*
	FROM 
		compWithFuels sf, dailyPriceWithUploadDates dudt
	Where 
		sf.CatNo = dudt.CatNo
		AND sf.FuelTypeId = dudt.FuelTypeId
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
,CompetitorsPrices AS
(
	Select cs.CompetitorId, cs.JsSiteId, cs.CatNo,
		cs.SiteName, cs.Address, cs.Suburb, cs.Town,  
		cs.IsSainsburysSite, cs.Brand, cs.Company, cs.Ownership,

		cs.Distance, cs.DriveTime, cs.Rank,
		cs.FuelTypeId, cs.FuelTypeName, 

		todp.UploadDateTime, 
		todp.ModalPrice,

		yestp.UploadDateTime UploadDateTimeYest, 
		yestp.ModalPrice ModalPriceYest
				
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
Order By JsSiteId, CompetitorId, DriveTime, Rank
