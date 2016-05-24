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



IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[UploadType])
BEGIN

INSERT [dbo].[UploadType] ([Id], [UploadTypeName]) VALUES (1, N'Daily Price Data')
INSERT [dbo].[UploadType] ([Id], [UploadTypeName]) VALUES (2, N'Quarterly Site Data')

END


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
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Mike',N'Gwyer',N'Mike.Gwyer@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Garry',N'Leeder',N'Garry.Leeder@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Premkumar',N'Krishnan',N'Premkumar.Krishnan@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'TestAdmin',N'Admin',N'testadmin@jsCoventryDev.onmicrosoft.com')


END
