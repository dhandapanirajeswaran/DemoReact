IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[ImportProcessStatus])
BEGIN

INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (1, N'Uploaded')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (2, N'Warning')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (5, N'Processing')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (10, N'Success')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (11, N'Calculating')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (12, N'CalcFailed')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (15, N'Failed')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (16, N'ImportAborted')
INSERT [dbo].[ImportProcessStatus] ([Id], [Status]) VALUES (17, N'CalcAborted')


END

---------------
--
-- Update/Insert the dbo.UploadType rows
--
declare @UploadTypeDataTV table(Id int, Name nvarchar(max))
insert into @UploadTypeDataTV
values 
	(1, N'Daily Price Data'),
	(2, N'Quarterly Site Data'),
	(3, N'Latest Js Price Data'),
	(4, N'Latest Competitors Price Data');

merge dbo.UploadType as target
using
(
	select Id, Name from @UploadTypeDataTV
) as source(Id, Name)
on target.Id = source.Id
when not matched then
	insert(Id, UploadTypeName)
	values(source.Id, source.Name)
when matched then
	update set target.UploadTypeName = source.Name;

---------------

IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[FuelType])
BEGIN
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (1, N'Super Unleaded')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (2, N'Unleaded')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (3, N'Unknown1')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (4, N'Unknown2')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (5, N'Super Diesel')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (6, N'Diesel')
INSERT [dbo].[FuelType] ([Id], [FuelTypeName]) VALUES (7, N'LPG')

END


IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[PPUser])
BEGIN

INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Izzy',N'Hexter',N'Izzy.Hexter@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Olivia',N'Darroch',N'Olivia.Darroch@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Simon',N'Millea',N'Simon.Millea@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Marjorie',N'Dehaney',N'Marjorie.Dehaney@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Owain',N'Fenn',N'Owain.Fenn@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Sandip',N'Vaidya',N'Sandip.Vaidya@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Garry',N'Leeder',N'Garry.Leeder@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Premkumar',N'Krishnan',N'Premkumar.Krishnan@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'TestAdmin',N'Admin',N'testadmin@jsCoventryDev.onmicrosoft.com')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Laura',N'Smith1',N'Laura.Smith1@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Ollie',N'Kemp',N'Ollie.Kemp@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Andy',N'Eaves',N'Andy.Eaves@sainsburys.co.uk')

END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsExcluded' AND Object_ID = Object_ID(N'SiteToCompetitor'))
BEGIN
 ALTER TABLE [dbo].[SiteToCompetitor] ADD [IsExcluded] int NOT NULL DEFAULT(0);
END

--
-- (re)-init the Sainsbury's store information (pfsno and StoreNo's)
--
EXEC [dbo].[spInitSainsburysStoreInformation]



IF NOT EXISTS(SELECT NULL FROM dbo.ContactDetails)
BEGIN
	INSERT INTO dbo.ContactDetails (Heading, Address, PhoneNumber, EmailName, EmailAddress, IsActive)
	VALUES ('Petrol Pricing Contact', 'Sainsburys Plc<br />33 Holborn, London,<br /> EC1N 2HT', '0207 69 52704', 'Product Owner', 'Izzy.Hexter@sainsburys.co.uk', 1);
END

--
-- PPUserPermissions - NOTE: The values are BITWISE but SQL refuses to evaluate 0x01 + 0x02 as 3 !!
--

DECLARE @Default_FileUploadsUserPermissions INT = 1 + 2 -- View and Upload
DECLARE @Default_SitePricingUserPermissions INT = 1 + 2 + 4 -- View, Export and Update
DECLARE @Default_SitesMaintenanceUserPermissions INT = 1 + 2 + 4 -- View, Add and Edit
DECLARE @Default_ReportsUserPermissions INT = 1 + 2 -- View and Export
DECLARE @Default_UsersManagementUserPermissions INT = 1 + 2 + 4 + 8 -- View, Add, Edit and Delete
DECLARE @Default_DiagnosticsUserPermissions INT = 0 -- None
DECLARE @Default_SystemSettingsUserPermissions INT = 1 + 2 -- View and Edit

MERGE
	dbo.PPUserPermissions AS target
	USING (
		SELECT
			usr.Id
		FROM
			dbo.PPUser usr
		WHERE
			NOT EXISTS(SELECT NULL FROM dbo.PPUserPermissions WHERE PPUserId = usr.Id)
	)
	AS source(PPUserId)
	ON (source.PPUserId = target.PPUserId)
	WHEN NOT MATCHED
		THEN 
		INSERT (
           [PPUserId],
           [IsAdmin],
           [FileUploadsUserPermissions],
           [SitePricingUserPermissions],
           [SitesMaintenanceUserPermissions],
           [ReportsUserPermissions],
           [UsersManagementUserPermissions],
           [DiagnosticsUserPermissions],
           [CreatedOn],
           [CreatedBy],
           [UpdatedOn],
           [UpdatedBy],
		   [SystemSettingsUserPermissions]
		   )
     VALUES
			(source.PPUserId,
			0, -- IsAdmin
			@Default_FileUploadsUserPermissions,
			@Default_SitePricingUserPermissions,
			@Default_SitesMaintenanceUserPermissions,
			@Default_ReportsUserPermissions,
			@Default_UsersManagementUserPermissions,
			@Default_DiagnosticsUserPermissions,
			GetDate(),
			0,
			GetDate(),
			0,
			@Default_SystemSettingsUserPermissions);

--
-- Setup normal users Settings access
--
UPDATE 
	dbo.PPUserPermissions
SET 
	SystemSettingsUserPermissions = @Default_SystemSettingsUserPermissions
WHERE 
	PPUserId IN (
	SELECT usr.Id
	FROM
		dbo.PPUser usr
	WHERE
		usr.Email IN (
			'Shilpa.Lathika@sainsburys.co.uk',
			'Izzy.Hexter@sainsburys.co.uk',
			'Olivia.Darroch@sainsburys.co.uk',
			'Ollie.Kemp@sainsburys.co.uk',
			'Laura.Smith1@sainsburys.co.uk',
			'Andy.Eaves@sainsburys.co.uk'
		)
	);

	--
-- Setup the Admins
--
UPDATE 
	dbo.PPUserPermissions
SET 
	IsAdmin = 1,
	DiagnosticsUserPermissions = 7,
	SystemSettingsUserPermissions = 3
WHERE 
	PPUserId IN (
	SELECT usr.Id
	FROM
		dbo.PPUser usr
	WHERE
		usr.Email IN (
			'Premkumar.Krishnan@sainsburys.co.uk', 
			'Garry.Leeder@sainsburys.co.uk', 
			'Sandip.Vaidya@sainsburys.co.uk',
			'Ollie.Kemp@sainsburys.co.uk'
		)
	);

--
-- Set up the Grocers
--

IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.Grocers)
BEGIN
	MERGE dbo.Grocers AS target
	USING (
		SELECT 'SAINSBURYS' [BrandName], 1 [IsSainsburys]
		UNION ALL
		SELECT 'ASDA', 0
		UNION ALL
		SELECT 'TESCO', 0
		UNION ALL
		SELECT 'MORRISONS', 0
	) AS source
	ON (source.BrandName = target.BrandName)
	WHEN NOT MATCHED
	THEN INSERT (BrandName, IsSainsburys)
		VALUES (source.BrandName, source.IsSainsburys)
	WHEN MATCHED
	THEN UPDATE SET
		BrandName = source.BrandName, 
		IsSainsburys = source.IsSainsburys;
END

--
-- Set up init SystemSettings (NOTE: other values have default constraints on the table itself)
--
IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.SystemSettings)
BEGIN
	INSERT INTO dbo.SystemSettings (DataCleanseFilesAfterDays, LastDataCleanseFilesOn)
	VALUES(60, NULL)
END

--
-- Default Email Template
--
MERGE dbo.EmailTemplate AS target
USING (
	SELECT 
		N'** Default Email Template' [TemplateName],
		N'{SiteName} - FAO Store/duty manager - URGENT FUEL PRICE CHANGE' [SubjectLine],
		N'<h1>FAO Store/duty manager - URGENT FUEL PRICE CHANGE</h1>
<p>Queries to the Trading Hotline using Option 1 Trading, Option 2 Grocery and Option 8 Petrol and Kiosk </p>
<h2>{SiteName}</h2>
<p><strong>Petrol price changes, effective end of trade {DayMonthYear}</strong></p>
<table>
	<tr><td><strong>Product</strong></td><td><strong>New Price</strong></td></tr>
	<tr><td>Unleaded</td><td>{UnleadedPrice}</td></tr>
	<tr><td>Super (if applicable)</td><td>{SuperPrice}</td></tr>
	<tr><td>Diesel</td><td>{DieselPrice}</td></tr>
</table>
<ul>
	<li>All fuel price changes must be actioned at the end of trade only</li>
	<li>Colleague actioning the price change must ensure the changes have applied on the pumps, enter the time of the change and then sign as confirmation before filing this message at the PFS</li>
	<li>Ensure you enter the new prices correctly into REPOS e.g. If the price is 129.90ppl, then enter 129.90 in the new price field (make sure there are no fuel sales outstanding on the REPOS system or pumps before making the price change, <strong>ALL</strong> working pumps <strong>MUST</strong> be left <strong>ON</strong> and nozzle placed in the pump holders)</li>
	<li>You are reminded that only fuel price changes sent to you by email should be actioned. Please ignore REPOS price change reports.</li>
</ul>
<h3>24 hour sites</h3>
<ul>
	<li>Action the price change in conjunction with the REPOS EOD routine between midnight and 2am. Colleague actioning the price change must ensure that the changes have applied on the pumps, enter the time of the change and then sign as confirmation before filing this message at the PFS.</li>
</ul>' [EmailBody]

) AS Source
ON (target.IsDefault = 1)
WHEN NOT MATCHED
THEN INSERT (IsDefault, TemplateName, SubjectLine, PPUserId, EmailBody)
	VALUES(1, source.TemplateName, source.SubjectLine, 0, source.EmailBody)
WHEN MATCHED
THEN UPDATE SET
	TemplateName = source.TemplateName,
	SubjectLine = source.SubjectLine,
	PPUserId = 0,
	EmailBody = source.EmailBody;

--
-- Fix Email Templates (map old tokens to new tokens)
--
UPDATE dbo.EmailTemplate 
SET EmailBody = REPLACE(EmailBody, '{StartDateMonthYear}', '{DayMonthYear}'),
	SubjectLine = REPLACE(REPLACE(SubjectLine, '{StartDateMonthYear}', '{DayMonthYear}'), '(Not sent to site)', '');

--
-- Seed [dbo].[DriveTimeMarkup]
--
DECLARE @DefaultDriveTimeMarkupsTV TABLE (DriveTime INT, Markup INT)
INSERT INTO 
	@DefaultDriveTimeMarkupsTV
VALUES 
	(0, 0),		--  0 to  4 minutes +0p
	(5, 1),		--  5 to  9 minutes +1p
	(10, 2),	-- 10 to 14 minutes +2p
	(15, 3),	-- 15 to 19 minutes +3p
	(20, 4),	-- 20 to 24 minutes +4p
	(25, 5)		-- 25 to 30 minutes +5p

IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.DriveTimeMarkup)
BEGIN
	INSERT INTO dbo.DriveTimeMarkup
	-- UNLEADED
	SELECT
		2, 
		ddtm.DriveTime, 
		ddtm.Markup
	FROM 
		@DefaultDriveTimeMarkupsTV ddtm
	-- DIESEL
	UNION ALL
	SELECT
		6,
		ddtm.DriveTime,
		ddtm.Markup
	FROM 
		@DefaultDriveTimeMarkupsTV ddtm
	-- SUPER-UNLEADED
	UNION ALL
	SELECT
		1,
		ddtm.DriveTime,
		ddtm.Markup
	FROM 
		@DefaultDriveTimeMarkupsTV ddtm		
END

--
-- seed [dbo].[PriceMatchType] table
--

SET IDENTITY_INSERT dbo.PriceMatchType ON;

MERGE
	dbo.PriceMatchType AS Target
	USING (
		SELECT '1', 'Latest'
		UNION ALL
		SELECT '2', 'Suggested Price'
		UNION ALL
		SELECT '3', 'Override Price'

	) AS Source (PriceMatchTypeId, PriceMatchTypeName)
	ON (source.PriceMatchTypeId = target.PriceMatchTypeId)
	WHEN NOT MATCHED THEN
		INSERT (
			[PriceMatchTypeId], 
			[PriceMatchTypeName]
		)
		VALUES (
			source.PriceMatchTypeId,
			source.PriceMatchTypeName
			)
	WHEN MATCHED THEN
		UPDATE SET
			PriceMatchTypeName = source.PriceMatchTypeName;

SET IDENTITY_INSERT dbo.PriceMatchType OFF;


--
-- Seed [dbo].[WinServiceEventType] table
--
SET IDENTITY_INSERT dbo.WinServiceEventType ON;
MERGE
	dbo.WinServiceEventType AS Target
	USING (
		SELECT '0', 'None'
		UNION ALL
		SELECT '1', 'DailyPriceEmail'

	) AS Source (WinServiceEventTypeId, EventTypeName)
	ON (Source.WinServiceEventTypeId = target.WinServiceEventTypeId)
	WHEN NOT MATCHED THEN
		INSERT (
			WinServiceEventTypeId,
			EventTypeName
		)
		VALUES (
			source.WinServiceEventTypeId,
			source.EventTypeName
		)
	WHEN MATCHED THEN
		UPDATE SET
			EventTypeName = source.EventTypeName;

SET IDENTITY_INSERT dbo.WinServiceEventType OFF;

--
-- Seed [dbo].[WinServiceEventStatus] table
--
SET IDENTITY_INSERT dbo.WinServiceEventStatus ON;

MERGE
	dbo.WinServiceEventStatus AS target
	USING (
		SELECT '0', 'None'
		UNION ALL
		SELECT '1', 'Paused'
		UNION ALL
		SELECT '2', 'Sleeping'
		UNION ALL
		SELECT '3', 'Running'
		UNION ALL
		SELECT '4', 'Success'
		UNION ALL
		SELECT '5', 'Failed'
	) AS Source(WinServiceEventStatusId,EventStatusName)
	ON (Source.WinServiceEventStatusId = target.WinServiceEventStatusId)
	WHEN NOT MATCHED THEN
		INSERT (
			WinServiceEventStatusId,
			EventStatusName
		)
		VALUES (
			source.WinServiceEventStatusId,
			source.EventStatusName
		)
	WHEN MATCHED THEN
		UPDATE SET
			EventStatusName = source.EventStatusName;

SET IDENTITY_INSERT dbo.WinServiceEventStatus OFF;


--
-- seed [dbo].[WinServiceSchedule] table
--
IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.WinServiceSchedule)
BEGIN
	INSERT INTO dbo.WinServiceSchedule
		(
			IsActive,
			WinServiceEventTypeId,
			ScheduledFor,
			LastPolledOn,
			LastStartedOn,
			LastCompletedOn,
			WinServiceEventStatusId,
			EmailAddress
		)
		VALUES (
			1,
			1, -- DailyPriceEmail
			DATEADD(HOUR, 18, CONVERT(DATETIME, CONVERT(DATE, GETDATE() + 1))), -- 18:00 hours next day
			NULL,
			NULL,
			NULL,
			2, -- Sleeping
			''
		);
END

--
-- Lookup BrandId for all Sites
--
EXEC dbo.spRebuildBrands

--
-- Lookup BrandIds
--
update gr 
set BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = gr.BrandName)
FROM dbo.Grocers gr;

update eb
set BrandId = (SELECT TOP 1 Id FROM dbo.Brand WHERE BrandName = eb.BrandName)
FROM dbo.ExcludeBrands eb;


--
-- Create the dbo.PriceReasonFlags table data
--
TRUNCATE TABLE dbo.pricereasonflags

INSERT INTO dbo.PriceReasonFlags (Id, BitMask, Descript)
VALUES	 (01, 0x00000001, 'Cheapest price Found')
		,(02, 0x00000002, 'Rounded')
		,(03, 0x00000004, 'Inside Price-Variance')
		,(04, 0x00000008, 'Outside Price-Variance')
		,(05, 0x00000010, 'Today Price SnapBack')
		,(06, 0x00000020, 'Has Grocers')
		,(07, 0x00000040, 'Has Incomplete Grocers')
		,(08, 0x00000080, 'Based on Unleaded')
		,(09, 0x00000100, 'Missing Site CatNo')
		,(10, 0x00000200, 'Missing Daily Catalist')
		,(11, 0x00000400, 'No Match-Competitor Price')
		,(12, 0x00000800, 'No Suggested Price')
		,(13, 0x00001000, 'Price Stunt Freeze')
		,(14, 0x00002000, 'Latest JS Price')
		,(15, 0x00004000, 'Manual Price Override')
		,(16, 0x00008000, 'Match Competitor Price Found')
		,(17, 0x00010000, 'Trial Price Found')

--
-- look Site Grocer and ExcludedBrand attributes
--
EXEC dbo.spRebuildSiteAttributes @SiteId = NULL -- Note: ALL sites

--
-- Determine PriceMatchType for existing sites
--

EXEC [dbo].[spSetSitePriceMatchTypeDefaults]

