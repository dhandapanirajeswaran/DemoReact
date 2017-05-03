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
           [UpdatedBy]
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
			0);

--
-- Setup the Admins
--
UPDATE 
	dbo.PPUserPermissions
SET 
	IsAdmin = 1,
	DiagnosticsUserPermissions = 7
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
-- Set up init SystemSettings
--
IF NOT EXISTS(SELECT TOP 1 NULL FROM dbo.SystemSettings)
BEGIN
	INSERT INTO dbo.SystemSettings (DataCleanseFilesAfterDays, LastDataCleanseFilesOn)
	VALUES(60, NULL)
END

--
-- Determine PriceMatchType for existing sites
--

EXEC [dbo].[spSetSitePriceMatchTypeDefaults]

