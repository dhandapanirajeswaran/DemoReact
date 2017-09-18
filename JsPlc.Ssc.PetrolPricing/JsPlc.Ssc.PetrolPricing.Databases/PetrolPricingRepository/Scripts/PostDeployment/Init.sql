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
-- Upsert dbo.ScheduleEmailTemplate records
--
MERGE dbo.ScheduleEmailTemplate AS target
USING (
	SELECT 
		1 [ScheduleEmailType],
		'(Automated) Sainsburys Petrol Pricing Daily Price File for ##DATE##' [SubjectLine],
		'olivia.darroch@sainsburys.co.uk' [ContactEmail],
		'
    <p><img src="data:image/png;base64,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" alt="Petrol Pricing" /></p>
    <p>Hi,</p>
    <p>This is an automated email sent out by the <strong>Sainsburys Petrol Pricing</strong> online application.</p>
    <p>The email is intended for <a href="##EMAIL-TO##">##EMAIL-TO##</a> and was generated on ##DATE## at ##TIME##.</p>
    <p>If you are *NOT* the intended recipient then please contact the following person:</p>
    <p><a href="mailto:##CONTACT-EMAIL##">##CONTACT-EMAIL##</a></p>
    <p>Thank you.</p>' [EmailBody]
) AS source (ScheduleEmailType, SubjectLine, ContactEmail, EmailBody)
ON (target.ScheduleEmailType = source.ScheduleEmailType)
WHEN MATCHED THEN
	UPDATE SET
		target.SubjectLine = source.SubjectLine,
		target.ContactEmail = source.ContactEmail,
		target.EmailBody = source.EmailBody
WHEN NOT MATCHED BY target THEN
	INSERT (ScheduleEmailType, SubjectLine, ContactEmail, EmailBody)
	VALUES (
		source.ScheduleEmailType,
		source.SubjectLine,
		source.ContactEmail,
		source.EmailBody
	);

--
-- look Site Grocer and ExcludedBrand attributes
--
EXEC dbo.spRebuildSiteAttributes @SiteId = NULL -- Note: ALL sites

--
-- Determine PriceMatchType for existing sites
--

EXEC [dbo].[spSetSitePriceMatchTypeDefaults]

