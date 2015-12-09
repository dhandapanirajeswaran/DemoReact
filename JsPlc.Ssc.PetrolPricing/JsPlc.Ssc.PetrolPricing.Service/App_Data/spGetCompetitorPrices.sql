CREATE PROCEDURE dbo.spGetCompetitorPrices
    @siteId int,
	@forDate DateTime
AS

--Declare @siteId int = 1
--Declare @forDate DateTime = '2015-11-30'

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

;With dailyPrices as
(
	Select dp.CatNo, dp.FuelTypeId, ft.FuelTypeName, fu.UploadDateTime, dp.ModalPrice, dp.AllStarMerchantNo
	FROM DailyPrice dp, FuelType ft, FileUpload fu
	Where dp.FuelTypeId = ft.Id
	And dp.DailyUploadId = fu.Id and fu.StatusId = 10 -- success files only
)
,todaysPrices as
(
	Select * from dailyPrices where DateDiff(day, UploadDateTime, @forDate) = 0
)
,yesterdaysPrices as
(
	Select * from dailyPrices where DateDiff(day, UploadDateTime, @forDate) = 1
)
,CompetitorsForSites AS -- competitor information with alongside respective JsSiteId
(
	Select sc.CompetitorId, sc.SiteId as JsSiteId, s.CatNo, 
		s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,

		sc.Distance, sc.DriveTime, sc.Rank
	FROM
	SiteToCompetitor sc
		inner join Site S
			On sc.CompetitorId = s.Id
)
,CompetitorsPrices AS
(
	Select cs.CompetitorId, cs.JsSiteId, cs.CatNo,
		cs.SiteName, cs.Address, cs.Suburb, cs.Town,  
		cs.IsSainsburysSite, cs.Brand, cs.Company, cs.Ownership,

		cs.Distance, cs.DriveTime, cs.Rank,

		todaysPrices.FuelTypeId, todaysPrices.FuelTypeName, 
		todaysPrices.UploadDateTime, 
		todaysPrices.ModalPrice,

		yesterdaysPrices.UploadDateTime UploadDateTimeYest, 
		yesterdaysPrices.ModalPrice ModalPriceYest
				
	From CompetitorsForSites cs
		Left Outer Join todaysPrices
			On cs.CatNo = todaysPrices.CatNo
		Left Outer Join yesterdaysPrices
			On cs.CatNo = yesterdaysPrices.CatNo
	Where cs.JsSiteId = @siteId
		and (todaysPrices.UploadDateTime is null OR DateDiff(day, todaysPrices.UploadDateTime, @forDate) = 0)
		and (yesterdaysPrices.UploadDateTime is null OR DateDiff(day, yesterdaysPrices.UploadDateTime, @forDate) = 1)
)
Select * from CompetitorsPrices Order By JsSiteId, CompetitorId, DriveTime, Rank
