CREATE PROCEDURE [dbo].[spGetSitePrices]
	@siteId int,
	@forDate DateTime,
	@skipRecs int,
	@takeRecs int,
	@storeName nvarchar(500),
	@storeTown nvarchar(500),
	@catNo int,
	@storeNo int
AS

----DEBUG:START
--DECLARE @siteId INT =982
--DECLARE @forDate DATETIME='2016-05-24 00:00:00'
--DECLARE @skipRecs INT=0
--DECLARE @takeRecs INT =2000
--DECLARE @storeName NVARCHAR(500)=N''
--DECLARE @storeTown NVARCHAR(500)=N''
--DECLARE @catNo INT =0
--DECLARE @storeNo INT=0
----DEBUG:END

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
   Select Distinct s.Id as SiteId, dp.FuelTypeId, ft.FuelTypeName, s.CompetitorPriceOffset
   from sites s, DailyPrice dp, FuelType ft
   Where s.CatNo = dp.CatNo and dp.FuelTypeId = ft.Id
   Union 
   Select Distinct s.Id as SiteId, sp.FuelTypeId, ft.FuelTypeName, s.CompetitorPriceOffset
   from sites s, SitePrice sp, FuelType ft
   Where s.Id = sp.SiteId and sp.FuelTypeId = ft.Id
) -- select * from siteFuels
,sitesWithFuels as
(
	Select s.Id as SiteId, s.CatNo, s.SiteName, s.Address, s.Suburb, s.Town,  
		s.IsSainsburysSite, s.Brand, s.Company, s.Ownership,
		sf.FuelTypeId, sf.FuelTypeName, s.PfsNo, s.StoreNo
	From 
		[Site] s 
			Inner Join siteFuels sf
				On s.Id = sf.SiteId
) -- select * from sitesWithFuels
,sitePrices as
(
	Select 
		sf.SiteId, sf.FuelTypeId, sf.FuelTypeName, 

		sp.DateOfCalc, sp.DateOfPrice, sp.EffDate,
		sp.SuggestedPrice, sp.OverriddenPrice, sp.CompetitorId, sp.Markup, sp.IsTrailPrice,
		sf.CompetitorPriceOffset
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
	SELECT swf.SiteId, swf.CatNo,
		swf.SiteName, swf.Address, swf.Suburb, swf.Town,  
		swf.IsSainsburysSite, swf.Brand, swf.Company, swf.Ownership,
		swf.FuelTypeId, swf.FuelTypeName, swf.PfsNo, swf.StoreNo,

		tomp.DateOfCalc, tomp.DateOfPrice, 

		CASE WHEN s.TrailPriceCompetitorId IS NOT NULL 
		THEN
			(SELECT TOP 1 tdp.ModalPrice FROM dbo.DailyPrice as tdp 
			inner join dbo.Site as ts on ts.CatNo = tdp.CatNo
			WHERE ts.TrailPriceCompetitorId = s.TrailPriceCompetitorId and tdp.FuelTypeId = swf.FuelTypeId)
			+ (todp.CompetitorPriceOffset * 10)
		ELSE tomp.SuggestedPrice
		END AS [SuggestedPrice],

		tomp.OverriddenPrice,

		CASE WHEN s.TrailPriceCompetitorId IS NOT NULL 
			THEN s.TrailPriceCompetitorId
			ELSE tomp.CompetitorId
		END as [CompetitorId],
		CASE WHEN s.TrailPriceCompetitorId IS NOT NULL
			THEN cast(todp.CompetitorPriceOffset as int)
			ELSE tomp.Markup
		END AS [Markup],

		--tomp.Markup,
		tomp.IsTrailPrice,

		todp.DateOfCalc DateOfCalcForTodaysPrice, todp.DateOfPrice DateOfPriceForTodaysPrice, 
		todp.SuggestedPrice SuggestedPriceToday,
		todp.OverriddenPrice OverriddenPriceToday,
		todp.CompetitorPriceOffset
	FROM 
		sitesWithFuels swf
		inner join dbo.Site as s on s.Id = swf.SiteId
		Left join tomorrowsPrices as tomp
			On swf.SiteId = tomp.SiteId
			And tomp.FuelTypeId = swf.FuelTypeId
		Left join todaysPrices as todp
			On swf.SiteId = todp.SiteId
			And todp.FuelTypeId = swf.FuelTypeId
	Where 
		(tomp.DateOfCalc is null OR DateDiff(day, tomp.DateOfCalc, @forDate) = 0)
		AND (todp.DateOfCalc is null OR DateDiff(day, todp.DateOfCalc, @todayPriceDate) = 0)
)
Select * from sitesWithPrices 
Order By SiteName