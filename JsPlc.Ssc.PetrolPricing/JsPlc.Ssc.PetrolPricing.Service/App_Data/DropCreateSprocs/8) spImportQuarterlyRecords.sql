CREATE PROCEDURE [dbo].[spImportQuarterlyRecords]
	@uploadId int
AS
-- USAGE: 
-- Exec [dbo].[spImportQuarterlyRecords] 5

Declare @msg NVarchar(2000)

Set @msg = 'File selection:='
-- Set @uploadId = (Select Top 1 QuarterlyUploadId from QuarterlyUploadStaging)

Print @msg + Convert(VARCHAR(10), @uploadId)
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, 'New Quarterly Import Run at:=' + Convert(VARCHAR(20), GetDate()))
-- Select @uploadId

-- End TODO
SET NOCOUNT ON;
--DECLARE @RowCount1 INTEGER

-- ALLSITES Table master
-- Drop if exists
Set @msg = 'Drop ##ALLSITES Master if exists'
Print @msg
-- GO
IF OBJECT_ID('tempdb..##ALLSites') IS NOT NULL /*Then it exists*/
   DROP TABLE ##ALLSites
-- GO
if (@@ERROR <> 0) return @@ERROR;

-- Create ALLSITES master
Set @msg = 'CREATE ##ALLSITES Master'
Print @msg
-- GO
CREATE TABLE ##ALLSites (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CatNo] [int] NULL,
	[Brand] [nvarchar](max) NULL,
	[SiteName] [nvarchar](max) NOT NULL,
	[Address] [nvarchar](max) NULL,
	[Suburb] [nvarchar](max) NULL,
	[Town] [nvarchar](max) NOT NULL,
	[PostCode] [nvarchar](max) NULL,
	[Company] [nvarchar](max) NULL,
	[Ownership] [nvarchar](max) NULL,
	[StoreNo] [int] NULL,
	[PfsNo] [int] NULL
 CONSTRAINT [PK_dbo.##ALLSites] PRIMARY KEY CLUSTERED (	[Id] ASC )
)
if (@@ERROR <> 0) return @@ERROR;

-- GO
-- Empty ALLSITES Master 
Set @msg = 'TRUNC ##ALLSITES Master'
Print @msg
-- GO
Truncate table ##ALLSites
-- GO
if (@@ERROR <> 0) return @@ERROR;

if (@@ERROR <> 0) return @@ERROR;


Set @msg = 'Populate ##ALLSITES Master - 1 of 2'
Print @msg
-- GO
-- Populate ALLSITES Master - 1 of 2 
Insert Into ##ALLSites (SiteName, Town, CatNo, Brand, Address, Suburb, PostCode, Company, Ownership)
	Select Distinct SiteName, Town, CatNo, 
	Brand, Addr, Suburb, PostCode, Company, Ownership 
	FROM QuarterlyUploadStaging -- Master data
if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;
-- End of Populate 1
-- GO

-- Populate ALLSITES Master - 2 of 2 
--	Insert to AllSites where ALLSites doesnt have that LEFT SLICE CatNo (no rows as per run)
Set @msg = 'Populate ##ALLSITES Master - 2 of 2'
Print @msg
-- GO
Insert ##ALLSites (SiteName, Town, CatNo, Brand, Address, Suburb, PostCode, Company, Ownership)
	Select Distinct SainsSiteName, SainsSiteTown, SainsSiteCatNo CatNo, 
	'SAINSBURYS' Brand, '' Addr, '' Suburb, '' PostCode, '' Company, '' Ownership 
	FROM QuarterlyUploadStaging q -- More catNos possibly
		LEFT Join ##ALLSites s
	ON q.CatNo = s.CatNo
	Where s.CatNo is null
if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;
-- End of Populate 2
-- GO

-- ALLSITES Master table is ready

-- 6135 total sites, 278 Sainsburys sites
--Select * from ##ALLSites Where Brand = 'SAINSBURYS'

-- #####################
--	SITE NEW SITE UPD
-- #####################

-- UPDATE missing CatNo for new Sites
Set @msg = 'UPDATE missing CatNo for new Sites'
Print @msg
-- GO
Update Site 
	Set 
		CatNo = master.CatNo
From Site s
	Inner Join -- YES Inner Join on Name
(Select * FROM ##ALLSITES Where Brand = 'SAINSBURYS' 
) master
On s.SiteName = master.SiteName
Where s.CatNo is null -- Where NewSite doesnt have a CatNo (shouldnt be assigned by user)

if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;
-- End of UPDATE Missing CatNo 
-- GO

-- #####################
--	SITE INS/UPD 
-- #####################

--Select * from Site -- 8 rows prior to Insert
--Select Distinct SiteName, PostCode from Site -- 6135 Distinct SiteNames-PostCodes, 6135 Distinct CatNos 

-- JOINED view (Full Outer Join) ##AllSites - Sites on CatNo (NOT Needed, did separate Left-Join-based-Inserts and Inner-Join-based-Updates)

-- SITE Inserts 
Set @msg = 'SITE Inserts'
Print @msg
-- GO
Insert SITE (SiteName, Town, CatNo, Brand, Address, Suburb, PostCode, Company, Ownership, IsSainsBurysSite, IsActive)
	Select a.SiteName, a.Town, a.CatNo, a.Brand, a.Address, a.Suburb, a.PostCode, a.Company, a.Ownership, 
		Case When a.Brand = 'SAINSBURYS' Then 1 ELSE 0 END as IsSainsBurysSite, 1 as IsActive
	FROM ##ALLSites a 
		LEFT Join SITE s -- Site records would be null for inserts
	ON a.CatNo = s.CatNo
	Where 1=1 
	AND s.CatNo is null 
	AND s.SiteName is null
	-- RUN Only Select - yeah 6127 Rows from Select when Site has 8 rows
if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;
-- End of SITE Inserts 
-- GO

-- Expecting 6135 - 8 = 6127 rows

-- Check duplicate SiteName records Where CatNos mismatch - IGNORE These as Distinct SiteNames makes little sense as we do have 2 BEECHLEY SERVICE STATIONs in diff postcodes

-- Checking Distinct SiteName, PostCode makes more sense..
--Select s1.Id, s1.SiteName, s1.CatNo, s2.Id as DupId, s2.SiteName as DupSiteName, s2.CatNo as DupCatNo 
--	from Site s1
--	Inner Join Site s2 on s1.SiteName = s2.SiteName And s1.Id <> s2.Id

-- SITE Updates
Set @msg = 'SITE Updates started'
Print @msg
-- GO
Update SITE 
	Set 
	SiteName = a.SiteName,
	Town = a.Town, 
	Brand = a.Brand, 
	Address = a.Address, 
	Suburb = a.Suburb, 
	PostCode = a.PostCode, 
	Company = a.Company,
	Ownership = a.Ownership
FROM
    ##AllSites a
INNER JOIN
    SITE s
ON 
    s.CatNo = a.CatNo -- For Matching records, update all Site data (except CatNo) from Catalist
Where -- Needed since we need performance (much better when less then 6135 rows affected)
	s.SiteName <> a.SiteName
	OR
	s.Town <> a.Town
	OR
	s.Brand <> a.Brand
	OR
	s.Address <> a.Address
	OR
	s.Suburb <> a.Suburb
	OR 
	s.PostCode <> a.PostCode
	OR
	s.Company <> a.Company
	OR
	s.Ownership <> a.Ownership
if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;

-- End of SITE Updates
-- GO

-- ###################################
--	SITEToCompetitor WIPE N Populate 
-- ###################################

-- Wipe
Set @msg = 'Wipe SiteToCompetitors table for repopulation'
Print @msg

Truncate Table SiteToCompetitor
if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done'
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;

-- Populate
-- SITEToCompetitor Inserts 
Set @msg = 'SITEToCompetitor Inserts/repopulation'
Print @msg
Insert Into SiteToCompetitor (SiteId, CompetitorId, Rank, Distance, DriveTime) 

	Select JsId, CompId, Rank, DriveDist, DriveTime from 
	( 
		-- Run the below select to Check Result being inserted
		Select s1.Id as JsId, s2.Id as CompId, s1.CatNo as JsCatNo, s2.CatNo as CompCatNo, q.Rank, q.DriveDist, q.DriveTime from 
			QuarterlyUploadStaging q -- use this as master now for SiteToCompetitor (now all CatNos from this are in Site) 
			Inner Join Site s1 -- JsSiteIds
			On s1.CatNo = q.SainsSiteCatNo
			Inner Join Site s2 -- Comp SiteIds
			On s2.CatNo = q.CatNo
		Where 1=1 
		AND q.SainsSiteCatNo is not null 
		AND q.CatNo is not null 
		AND q.DriveDist is not null 
		AND q.DriveTime is not null 
		AND q.Rank is not null 
		-- And s1.CatNo = 100 -- Uncomment and run Select to check rows for single Js site
	) x

if (@@ERROR <> 0) return @@ERROR;

Set @msg = @msg + ' done, rows:' + Convert(VARCHAR(10), @@ROWCOUNT)
Print @msg
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, @msg)
if (@@ERROR <> 0) return @@ERROR;

-- End of Quarterly Import Process
Insert Into ImportProcessError (UploadId, RowOrLineNumber, ErrorMessage) Values(@uploadId, 0, 'Quarterly Import Run Completed at:=' + Convert(VARCHAR(20), GetDate()))
if (@@ERROR <> 0) return @@ERROR;

-- Run time reported = 1 sec
Print 'Import Successful'

-- ROLLBACK TRAN -- EUGH This will rollback the ImportErrors logging, improv later to collect logging items in a temp table and log from that

-- Convention Return 0 indicates success and a nonzero value indicates failure.
Return 0;

