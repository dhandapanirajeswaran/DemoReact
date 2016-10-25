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

Declare @todayPriceDate DateTime, @lastPriceDate DateTime = null, @YesterDayDate Datetime=null
set @todayPriceDate =@forDate
set @YesterDayDate =DateAdd(day, -1, @forDate)
--Select @todayPriceDate

Declare @DataExits INT=(select COUNT(*) from SitePrice where DateDiff(day, DateOfPrice, @forDate) = 0)
Declare @UploadID INT = (select TOP 1 Id from FileUpload where DATEPART(YY, UploadDateTime)=DATEPART(YY, @todayPriceDate) and DATEPART(MM, UploadDateTime)=DATEPART(MM, @todayPriceDate) and  DATEPART(DD, UploadDateTime)=DATEPART(DD, @todayPriceDate) and StatusId=10 and UploadTypeId=1 order by Id desc)


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
,sitePrices as
(
 select s.*, ft.FuelTypeName, ft.Id as FuelTypeId, sp.SuggestedPrice, sp.OverriddenPrice,sp.CompetitorId,sp.Markup,sp.IsTrailPrice,sp.DateOfPrice as DateOfPriceForTodaysPrice, dp.ModalPrice as SuggestedPriceToday  from sites s 
    inner join siteprice sp on  ( sp.SiteId=s.Id  and DateDiff(day, sp.DateOfPrice, @YesterDayDate) = 0 and sp.UploadId=@UploadID  )
	 left join Dailyprice dp on  (dp.CatNo=s.CatNo and dp.DailyUploadId =@UploadID and dp.FuelTypeId=sp.FuelTypeId and DateDiff(day, dp.DateOfPrice, @YesterDayDate) = 0)
	 inner join FuelType ft on  ft.Id= sp.FuelTypeId
)
Select distinct * from sitePrices 
Order By SiteName