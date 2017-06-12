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
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Ramaraju',N'Vittanala',N'Ramaraju.Vittanala@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'TestAdmin',N'Admin',N'testadmin@jsCoventryDev.onmicrosoft.com')

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
			'Ramaraju.Vittanala@sainsburys.co.uk', 
			'Garry.Leeder@sainsburys.co.uk', 
			'Sandip.Vaidya@sainsburys.co.uk'
		)
	);

--
-- Set up the Grocers
--


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
		N'{SiteName} - (Not sent to site)FAO Store/duty manager - URGENT FUEL PRICE CHANGE' [SubjectLine],
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
	SubjectLine = REPLACE(SubjectLine, '{StartDateMonthYear}', '{DayMonthYear}');


--
-- Determine PriceMatchType for existing sites
--

EXEC [dbo].[spSetSitePriceMatchTypeDefaults]

