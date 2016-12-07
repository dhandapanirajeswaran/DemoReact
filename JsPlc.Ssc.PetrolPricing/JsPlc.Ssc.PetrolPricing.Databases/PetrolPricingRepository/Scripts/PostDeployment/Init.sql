﻿IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[ImportProcessStatus])
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
INSERT [dbo].[UploadType] ([Id], [UploadTypeName]) VALUES (3, N'Latest Js Price Data')
INSERT [dbo].[UploadType] ([Id], [UploadTypeName]) VALUES (4, N'Latest Competitors Price Data')
END
update [dbo].[UploadType] set [UploadTypeName]= N'Latest Js Price Data' where [Id]=3

IF NOT EXISTS(SELECT TOP 1 1 FROM [dbo].[UploadType] where Id=4)
BEGIN

INSERT [dbo].[UploadType] ([Id], [UploadTypeName]) VALUES (4, N'Latest Competitors Price Data')
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
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'Ramaraju',N'Vittanala',N'Ramaraju.Vittanala@sainsburys.co.uk')
INSERT [dbo].[PPUser] ([FirstName],[LastName],[Email]) VALUES (N'TestAdmin',N'Admin',N'testadmin@jsCoventryDev.onmicrosoft.com')


IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsExcluded' AND Object_ID = Object_ID(N'SiteToCompetitor'))
BEGIN
 ALTER TABLE [dbo].[SiteToCompetitor] ADD [IsExcluded] int NOT NULL DEFAULT(0);
END

END



update [dbo].[Site] set pfsno=578, StoreNo=524 where SiteName like '%SAINSBURYS Aberdeen%'
update [dbo].[Site] set pfsno=196, StoreNo=646 where SiteName like '%SAINSBURYS Alperton%'
update [dbo].[Site] set pfsno=1006, StoreNo=2010 where SiteName like '%SAINSBURYS Alphington Road%'
update [dbo].[Site] set pfsno=200, StoreNo=662 where SiteName like '%SAINSBURYS Alton%'
update [dbo].[Site] set pfsno=456, StoreNo=722 where SiteName like '%SAINSBURYS Amblecote%'
update [dbo].[Site] set pfsno=64, StoreNo=634 where SiteName like '%SAINSBURYS Apsley Mill%'
update [dbo].[Site] set pfsno=746, StoreNo=832 where SiteName like '%SAINSBURYS Archer Road%'
update [dbo].[Site] set pfsno=1100, StoreNo=2100 where SiteName like '%SAINSBURYS Arnold%'
update [dbo].[Site] set pfsno=1054, StoreNo=2054 where SiteName like '%SAINSBURYS Ashton Moss%'
update [dbo].[Site] set pfsno=165, StoreNo=666 where SiteName like '%SAINSBURYS Badgers Farm%'
update [dbo].[Site] set pfsno=96, StoreNo=673 where SiteName like '%SAINSBURYS Bagshot%'
update [dbo].[Site] set pfsno=586, StoreNo=537 where SiteName like '%SAINSBURYS Ballymena%'
update [dbo].[Site] set pfsno=155, StoreNo=894 where SiteName like '%SAINSBURYS Bamber bridge%'
update [dbo].[Site] set pfsno=459, StoreNo=642 where SiteName like '%SAINSBURYS Banbury%'
update [dbo].[Site] set pfsno=210, StoreNo=674 where SiteName like '%SAINSBURYS Barnstaple%'
update [dbo].[Site] set pfsno=150, StoreNo=608 where SiteName like '%SAINSBURYS Barnwood%'
update [dbo].[Site] set pfsno=1008, StoreNo=5 where SiteName like '%SAINSBURYS Bath%'
update [dbo].[Site] set pfsno=563, StoreNo=569 where SiteName like '%SAINSBURYS Beckton%'
update [dbo].[Site] set pfsno=477, StoreNo=859 where SiteName like '%SAINSBURYS Beeston%'
update [dbo].[Site] set pfsno=179, StoreNo=824 where SiteName like '%SAINSBURYS Belgrave Road%'
update [dbo].[Site] set pfsno=439, StoreNo=426 where SiteName like '%SAINSBURYS Biggleswade%'
update [dbo].[Site] set pfsno=137, StoreNo=717 where SiteName like '%SAINSBURYS Birkenhead%'
update [dbo].[Site] set pfsno=223, StoreNo=752 where SiteName like '%SAINSBURYS Blackhall%'
update [dbo].[Site] set pfsno=592, StoreNo=541 where SiteName like '%SAINSBURYS Blackheath%'
update [dbo].[Site] set pfsno=127, StoreNo=53 where SiteName like '%SAINSBURYS Blackpole%'
update [dbo].[Site] set pfsno=1835, StoreNo=2835 where SiteName like '%SAINSBURYS Bolton%'
update [dbo].[Site] set pfsno=449, StoreNo=814 where SiteName like '%SAINSBURYS Bradford%'
update [dbo].[Site] set pfsno=1231, StoreNo=2231 where SiteName like '%SAINSBURYS Braehead%'
update [dbo].[Site] set pfsno=896, StoreNo=224 where SiteName like '%SAINSBURYS Braintree%'
update [dbo].[Site] set pfsno=86, StoreNo=505 where SiteName like '%SAINSBURYS Bramingham Park%'
update [dbo].[Site] set pfsno=238, StoreNo=669 where SiteName like '%SAINSBURYS Bridgemead%'
update [dbo].[Site] set pfsno=486, StoreNo=643 where SiteName like '%SAINSBURYS Bridgend%'
update [dbo].[Site] set pfsno=159, StoreNo=667 where SiteName like '%SAINSBURYS Bridgwater%'
update [dbo].[Site] set pfsno=80, StoreNo=681 where SiteName like '%SAINSBURYS Broadcut%'
update [dbo].[Site] set pfsno=487, StoreNo=686 where SiteName like '%SAINSBURYS Brookwood%'
update [dbo].[Site] set pfsno=591, StoreNo=652 where SiteName like '%SAINSBURYS Burpham%'
update [dbo].[Site] set pfsno=1001, StoreNo=411 where SiteName like '%SAINSBURYS Bury St Edmunds%'
update [dbo].[Site] set pfsno=91, StoreNo=59 where SiteName like '%SAINSBURYS Bybrook%'
update [dbo].[Site] set pfsno=162, StoreNo=559 where SiteName like '%SAINSBURYS Calcot%'
update [dbo].[Site] set pfsno=476, StoreNo=773 where SiteName like '%SAINSBURYS Canley%'
update [dbo].[Site] set pfsno=239, StoreNo=775 where SiteName like '%SAINSBURYS Cannock%'
update [dbo].[Site] set pfsno=1134, StoreNo=2134 where SiteName like '%SAINSBURYS Canvey Island%'
update [dbo].[Site] set pfsno=148, StoreNo=813 where SiteName like '%SAINSBURYS Castle Boulevard%'
update [dbo].[Site] set pfsno=785, StoreNo=795 where SiteName like '%SAINSBURYS Castle Court%'
update [dbo].[Site] set pfsno=1106, StoreNo=2106 where SiteName like '%SAINSBURYS Castle Vale%'
update [dbo].[Site] set pfsno=240, StoreNo=751 where SiteName like '%SAINSBURYS Chaddesden%'
update [dbo].[Site] set pfsno=1071, StoreNo=2071 where SiteName like '%SAINSBURYS Chafford Hundred%'
update [dbo].[Site] set pfsno=457, StoreNo=772 where SiteName like '%SAINSBURYS Cheadle%'
update [dbo].[Site] set pfsno=134, StoreNo=4 where SiteName like '%SAINSBURYS Chertsey%'
update [dbo].[Site] set pfsno=139, StoreNo=721 where SiteName like '%SAINSBURYS Chester%'
update [dbo].[Site] set pfsno=178, StoreNo=849 where SiteName like '%SAINSBURYS Chesterfield%'
update [dbo].[Site] set pfsno=185, StoreNo=6 where SiteName like '%SAINSBURYS Chichester%'
update [dbo].[Site] set pfsno=233, StoreNo=664 where SiteName like '%SAINSBURYS Chippenham%'
update [dbo].[Site] set pfsno=473, StoreNo=687 where SiteName like '%SAINSBURYS Cobham%'
update [dbo].[Site] set pfsno=138, StoreNo=8 where SiteName like '%SAINSBURYS Colchester Ave%'
update [dbo].[Site] set pfsno=126, StoreNo=815 where SiteName like '%SAINSBURYS Coldhams Lane%'
update [dbo].[Site] set pfsno=583, StoreNo=534 where SiteName like '%SAINSBURYS Coleraine%'
update [dbo].[Site] set pfsno=97, StoreNo=434 where SiteName like '%SAINSBURYS Corey%'
update [dbo].[Site] set pfsno=176, StoreNo=789 where SiteName like '%SAINSBURYS CourtHouse Green%'
update [dbo].[Site] set pfsno=1010, StoreNo=2013 where SiteName like '%SAINSBURYS Craigavon%'
update [dbo].[Site] set pfsno=1073, StoreNo=2073 where SiteName like '%SAINSBURYS Cramlington%'
update [dbo].[Site] set pfsno=145, StoreNo=17 where SiteName like '%SAINSBURYS Crayford%'
update [dbo].[Site] set pfsno=221, StoreNo=639 where SiteName like '%SAINSBURYS Cwmbran%'
update [dbo].[Site] set pfsno=197, StoreNo=885 where SiteName like '%SAINSBURYS Darnley%'
update [dbo].[Site] set pfsno=189, StoreNo=749 where SiteName like '%SAINSBURYS Deepdale%'
update [dbo].[Site] set pfsno=174, StoreNo=886 where SiteName like '%SAINSBURYS Denton%'
update [dbo].[Site] set pfsno=218, StoreNo=897 where SiteName like '%SAINSBURYS Dewsbury%'
update [dbo].[Site] set pfsno=1067, StoreNo=2067 where SiteName like '%SAINSBURYS Didcot%'
update [dbo].[Site] set pfsno=458, StoreNo=601 where SiteName like '%SAINSBURYS Dome Roundabout%'
update [dbo].[Site] set pfsno=577, StoreNo=523 where SiteName like '%SAINSBURYS Drumchapel%'
update [dbo].[Site] set pfsno=1549, StoreNo=549 where SiteName like '%SAINSBURYS Dundee%'
update [dbo].[Site] set pfsno=89, StoreNo=611 where SiteName like '%SAINSBURYS Dunstable%'
update [dbo].[Site] set pfsno=232, StoreNo=776 where SiteName like '%SAINSBURYS Durham%'
update [dbo].[Site] set pfsno=492, StoreNo=797 where SiteName like '%SAINSBURYS East Filton%'
update [dbo].[Site] set pfsno=493, StoreNo=893 where SiteName like '%SAINSBURYS East Kilbride%'
update [dbo].[Site] set pfsno=169, StoreNo=443 where SiteName like '%SAINSBURYS East Mayne%'
update [dbo].[Site] set pfsno=82, StoreNo=765 where SiteName like '%SAINSBURYS East Prescot Road%'
update [dbo].[Site] set pfsno=325, StoreNo=560 where SiteName like '%SAINSBURYS Cameron Toll%'
update [dbo].[Site] set pfsno=192, StoreNo=744 where SiteName like '%SAINSBURYS Ellesmere Port%'
update [dbo].[Site] set pfsno=481, StoreNo=7 where SiteName like '%SAINSBURYS Eltham%'
update [dbo].[Site] set pfsno=168, StoreNo=677 where SiteName like '%SAINSBURYS Emersons Green%'
update [dbo].[Site] set pfsno=95, StoreNo=444 where SiteName like '%SAINSBURYS Enfield%'
update [dbo].[Site] set pfsno=77, StoreNo=609 where SiteName like '%SAINSBURYS Fairfield Park%'
update [dbo].[Site] set pfsno=81, StoreNo=672 where SiteName like '%SAINSBURYS Farlington%'
update [dbo].[Site] set pfsno=465, StoreNo=656 where SiteName like '%SAINSBURYS Ferndown%'
update [dbo].[Site] set pfsno=584, StoreNo=535 where SiteName like '%SAINSBURYS Forestside%'
update [dbo].[Site] set pfsno=92, StoreNo=727 where SiteName like '%SAINSBURYS Fosse Park%'
update [dbo].[Site] set pfsno=120, StoreNo=685 where SiteName like '%SAINSBURYS Frome%'
update [dbo].[Site] set pfsno=461, StoreNo=543 where SiteName like '%SAINSBURYS Glen Road%'
update [dbo].[Site] set pfsno=482, StoreNo=670 where SiteName like '%SAINSBURYS Godalming%'
update [dbo].[Site] set pfsno=1081, StoreNo=2081 where SiteName like '%SAINSBURYS Grantham%'
update [dbo].[Site] set pfsno=1625, StoreNo=2625 where SiteName like '%SAINSBURYS Charlton Riverside%'
update [dbo].[Site] set pfsno=149, StoreNo=892 where SiteName like '%SAINSBURYS Grimsby%'
update [dbo].[Site] set pfsno=432, StoreNo=422 where SiteName like '%SAINSBURYS Hadleigh Road%'
update [dbo].[Site] set pfsno=597, StoreNo=778 where SiteName like '%SAINSBURYS Halifax%'
update [dbo].[Site] set pfsno=836, StoreNo=803 where SiteName like '%SAINSBURYS Hamilton%'
update [dbo].[Site] set pfsno=99, StoreNo=62 where SiteName like '%SAINSBURYS Hampden Park%'
update [dbo].[Site] set pfsno=94, StoreNo=709 where SiteName like '%SAINSBURYS Hankridge Farm%'
update [dbo].[Site] set pfsno=114, StoreNo=869 where SiteName like '%SAINSBURYS Hanley%'
update [dbo].[Site] set pfsno=464, StoreNo=402 where SiteName like '%SAINSBURYS Harlow%'
update [dbo].[Site] set pfsno=483, StoreNo=406 where SiteName like '%SAINSBURYS Harringay%'
update [dbo].[Site] set pfsno=158, StoreNo=711 where SiteName like '%SAINSBURYS Harrogate%'
update [dbo].[Site] set pfsno=234, StoreNo=419 where SiteName like '%SAINSBURYS Haverhill%'
update [dbo].[Site] set pfsno=75, StoreNo=630 where SiteName like '%SAINSBURYS Hayes%'
update [dbo].[Site] set pfsno=1268, StoreNo=2268 where SiteName like '%SAINSBURYS Heaton Newcastle%'
update [dbo].[Site] set pfsno=1288, StoreNo=2288 where SiteName like '%SAINSBURYS Heaton Park%'
update [dbo].[Site] set pfsno=151, StoreNo=657 where SiteName like '%SAINSBURYS Hedge End%'
update [dbo].[Site] set pfsno=520, StoreNo=556 where SiteName like '%SAINSBURYS Hempstead%'
update [dbo].[Site] set pfsno=66, StoreNo=637 where SiteName like '%SAINSBURYS Hendon%'
update [dbo].[Site] set pfsno=479, StoreNo=519 where SiteName like '%SAINSBURYS Hereford%'
update [dbo].[Site] set pfsno=180, StoreNo=600 where SiteName like '%SAINSBURYS Heyford Hill%'
update [dbo].[Site] set pfsno=1015, StoreNo=2005 where SiteName like '%SAINSBURYS Hoddesdon%'
update [dbo].[Site] set pfsno=1114, StoreNo=2114 where SiteName like '%SAINSBURYS Holywood Exchange%'
update [dbo].[Site] set pfsno=451, StoreNo=391 where SiteName like '%SAINSBURYS Horsham%'
update [dbo].[Site] set pfsno=141, StoreNo=825 where SiteName like '%SAINSBURYS Hull%'
update [dbo].[Site] set pfsno=1107, StoreNo=2007 where SiteName like '%SAINSBURYS Huntingdon%'
update [dbo].[Site] set pfsno=1105, StoreNo=2105 where SiteName like '%SAINSBURYS Isle of Wight%'
update [dbo].[Site] set pfsno=587, StoreNo=847 where SiteName like '%SAINSBURYS Keighley%'
update [dbo].[Site] set pfsno=152, StoreNo=665 where SiteName like '%SAINSBURYS Kempshott%'
update [dbo].[Site] set pfsno=497, StoreNo=757 where SiteName like '%SAINSBURYS Kempston%'
update [dbo].[Site] set pfsno=431, StoreNo=628 where SiteName like '%SAINSBURYS Kettering%'
update [dbo].[Site] set pfsno=107, StoreNo=640 where SiteName like '%SAINSBURYS Kidderminster%'
update [dbo].[Site] set pfsno=184, StoreNo=631 where SiteName like '%SAINSBURYS Kidlington%'
update [dbo].[Site] set pfsno=68, StoreNo=683 where SiteName like '%SAINSBURYS Kiln Lane%'
update [dbo].[Site] set pfsno=469, StoreNo=816 where SiteName like '%SAINSBURYS Kimberley%'
update [dbo].[Site] set pfsno=147, StoreNo=871 where SiteName like '%SAINSBURYS Kingsway%'
update [dbo].[Site] set pfsno=491, StoreNo=2001 where SiteName like '%SAINSBURYS Kirkcaldy%'
update [dbo].[Site] set pfsno=132, StoreNo=602 where SiteName like '%SAINSBURYS Ladbroke Grove%'
update [dbo].[Site] set pfsno=494, StoreNo=28 where SiteName like '%SAINSBURYS Larkfield%'
update [dbo].[Site] set pfsno=173, StoreNo=503 where SiteName like '%SAINSBURYS Leamington%'
update [dbo].[Site] set pfsno=575, StoreNo=695 where SiteName like '%SAINSBURYS Leeds Savacentre%'
update [dbo].[Site] set pfsno=1005, StoreNo=70 where SiteName like '%SAINSBURYS Leigh%'
update [dbo].[Site] set pfsno=235, StoreNo=421 where SiteName like '%SAINSBURYS Letchworth%'
update [dbo].[Site] set pfsno=1009, StoreNo=2017 where SiteName like '%SAINSBURYS Leven%'
update [dbo].[Site] set pfsno=111, StoreNo=725 where SiteName like '%SAINSBURYS Lincoln%'
update [dbo].[Site] set pfsno=1011, StoreNo=2023 where SiteName like '%SAINSBURYS Liphook%'
update [dbo].[Site] set pfsno=396, StoreNo=567 where SiteName like '%SAINSBURYS London Colney%'
update [dbo].[Site] set pfsno=452, StoreNo=706 where SiteName like '%SAINSBURYS Longwater%'
update [dbo].[Site] set pfsno=131, StoreNo=18 where SiteName like '%SAINSBURYS LordsHill%'
update [dbo].[Site] set pfsno=478, StoreNo=852 where SiteName like '%SAINSBURYS Loughborough%'
update [dbo].[Site] set pfsno=108, StoreNo=433 where SiteName like '%SAINSBURYS Low Hall%'
update [dbo].[Site] set pfsno=213, StoreNo=58 where SiteName like '%SAINSBURYS Lyons Farm%'
update [dbo].[Site] set pfsno=1269, StoreNo=2269 where SiteName like '%SAINSBURYS Mansfield%'
update [dbo].[Site] set pfsno=85, StoreNo=605 where SiteName like '%SAINSBURYS Market Harborough%'
update [dbo].[Site] set pfsno=438, StoreNo=691 where SiteName like '%SAINSBURYS Marsh Mills%'
update [dbo].[Site] set pfsno=498, StoreNo=714 where SiteName like '%SAINSBURYS Marshall Lake%'
update [dbo].[Site] set pfsno=1038, StoreNo=2112 where SiteName like '%SAINSBURYS Meadowhall North%'
update [dbo].[Site] set pfsno=582, StoreNo=521 where SiteName like '%SAINSBURYS Melksham%'
update [dbo].[Site] set pfsno=394, StoreNo=566 where SiteName like '%SAINSBURYS Merton%'
update [dbo].[Site] set pfsno=175, StoreNo=790 where SiteName like '%SAINSBURYS Middlesbrough%'
update [dbo].[Site] set pfsno=193, StoreNo=713 where SiteName like '%SAINSBURYS Monks Cross%'
update [dbo].[Site] set pfsno=1170, StoreNo=2170 where SiteName like '%SAINSBURYS Nantwich%'
update [dbo].[Site] set pfsno=692, StoreNo=31 where SiteName like '%SAINSBURYS New Cross Gate%'
update [dbo].[Site] set pfsno=488, StoreNo=690 where SiteName like '%SAINSBURYS Newbury%'
update [dbo].[Site] set pfsno=446, StoreNo=2136 where SiteName like '%SAINSBURYS Newhaven%'
update [dbo].[Site] set pfsno=1246, StoreNo=2246 where SiteName like '%SAINSBURYS Newport%'
update [dbo].[Site] set pfsno=525, StoreNo=538 where SiteName like '%SAINSBURYS Newry%'
update [dbo].[Site] set pfsno=110, StoreNo=882 where SiteName like '%SAINSBURYS Newton Abbot%'
update [dbo].[Site] set pfsno=485, StoreNo=225 where SiteName like '%SAINSBURYS Nine Elms%'
update [dbo].[Site] set pfsno=495, StoreNo=38 where SiteName like '%SAINSBURYS North Cheam%'
update [dbo].[Site] set pfsno=1046, StoreNo=2046 where SiteName like '%SAINSBURYS North Walsham%'
update [dbo].[Site] set pfsno=480, StoreNo=762 where SiteName like '%SAINSBURYS Northwich%'
update [dbo].[Site] set pfsno=112, StoreNo=558 where SiteName like '%SAINSBURYS Oldbury%'
update [dbo].[Site] set pfsno=1063, StoreNo=2063 where SiteName like '%SAINSBURYS Oldham%'
update [dbo].[Site] set pfsno=1059, StoreNo=2059 where SiteName like '%SAINSBURYS Osmaston Park%'
update [dbo].[Site] set pfsno=104, StoreNo=801 where SiteName like '%SAINSBURYS Paignton%'
update [dbo].[Site] set pfsno=76, StoreNo=3 where SiteName like '%SAINSBURYS Pepper Hill%'
update [dbo].[Site] set pfsno=136, StoreNo=823 where SiteName like '%SAINSBURYS Perton%'
update [dbo].[Site] set pfsno=182, StoreNo=417 where SiteName like '%SAINSBURYS Peterborough%'
update [dbo].[Site] set pfsno=190, StoreNo=682 where SiteName like '%SAINSBURYS Pinhoe Road%'
update [dbo].[Site] set pfsno=230, StoreNo=441 where SiteName like '%SAINSBURYS Pound Lane%'
update [dbo].[Site] set pfsno=157, StoreNo=52 where SiteName like '%SAINSBURYS Purley Way%'
update [dbo].[Site] set pfsno=594, StoreNo=413 where SiteName like '%SAINSBURYS Queens Road%'
update [dbo].[Site] set pfsno=470, StoreNo=72 where SiteName like '%SAINSBURYS Rayleigh Weir%'
update [dbo].[Site] set pfsno=181, StoreNo=604 where SiteName like '%SAINSBURYS Redditch%'
update [dbo].[Site] set pfsno=161, StoreNo=853 where SiteName like '%SAINSBURYS Reedswood%'
update [dbo].[Site] set pfsno=78, StoreNo=633 where SiteName like '%SAINSBURYS Rhyl%'
update [dbo].[Site] set pfsno=1004, StoreNo=2011 where SiteName like '%SAINSBURYS Rice Lane%'
update [dbo].[Site] set pfsno=1548, StoreNo=548 where SiteName like '%SAINSBURYS Richmond%'
update [dbo].[Site] set pfsno=113, StoreNo=742 where SiteName like '%SAINSBURYS Ripley%'
update [dbo].[Site] set pfsno=1696, StoreNo=696 where SiteName like '%SAINSBURYS Romford%'
update [dbo].[Site] set pfsno=177, StoreNo=514 where SiteName like '%SAINSBURYS Rugby%'
update [dbo].[Site] set pfsno=231, StoreNo=873 where SiteName like '%SAINSBURYS Salford%'
update [dbo].[Site] set pfsno=1080, StoreNo=2080 where SiteName like '%SAINSBURYS Scarborough%'
update [dbo].[Site] set pfsno=472, StoreNo=27 where SiteName like '%SAINSBURYS Sedlescombe Road%'
update [dbo].[Site] set pfsno=466, StoreNo=867 where SiteName like '%SAINSBURYS Leeds Colton%'
update [dbo].[Site] set pfsno=1000, StoreNo=63 where SiteName like '%SAINSBURYS Sevenoaks%'
update [dbo].[Site] set pfsno=90, StoreNo=793 where SiteName like '%SAINSBURYS Shorehead%'
update [dbo].[Site] set pfsno=109, StoreNo=718 where SiteName like '%SAINSBURYS Shrewsbury%'
update [dbo].[Site] set pfsno=1644, StoreNo=644 where SiteName like '%SAINSBURYS South Ruislip%'
update [dbo].[Site] set pfsno=1079, StoreNo=2079 where SiteName like '%SAINSBURYS Spalding%'
update [dbo].[Site] set pfsno=187, StoreNo=415 where SiteName like '%SAINSBURYS Springfield%'
update [dbo].[Site] set pfsno=1061, StoreNo=2061 where SiteName like '%SAINSBURYS Sprucefield%'
update [dbo].[Site] set pfsno=88, StoreNo=629 where SiteName like '%SAINSBURYS St Albans%'
update [dbo].[Site] set pfsno=219, StoreNo=661 where SiteName like '%SAINSBURYS St Clares%'
update [dbo].[Site] set pfsno=468, StoreNo=679 where SiteName like '%SAINSBURYS Staines%'
update [dbo].[Site] set pfsno=1168, StoreNo=2168 where SiteName like '%SAINSBURYS Stanway%'
update [dbo].[Site] set pfsno=133, StoreNo=410 where SiteName like '%SAINSBURYS Stevenage%'
update [dbo].[Site] set pfsno=580, StoreNo=529 where SiteName like '%SAINSBURYS Stirling%'
update [dbo].[Site] set pfsno=1113, StoreNo=2113 where SiteName like '%SAINSBURYS Stoke%'
update [dbo].[Site] set pfsno=877, StoreNo=812 where SiteName like '%SAINSBURYS Straiton%'
update [dbo].[Site] set pfsno=146, StoreNo=745 where SiteName like '%SAINSBURYS Stratton%'
update [dbo].[Site] set pfsno=754, StoreNo=788 where SiteName like '%SAINSBURYS Street%'
update [dbo].[Site] set pfsno=216, StoreNo=774 where SiteName like '%SAINSBURYS Sunderland%'
update [dbo].[Site] set pfsno=467, StoreNo=753 where SiteName like '%SAINSBURYS Swadlincote%'
update [dbo].[Site] set pfsno=1003, StoreNo=510 where SiteName like '%SAINSBURYS Swansea%'
update [dbo].[Site] set pfsno=565, StoreNo=693 where SiteName like '%SAINSBURYS Sydenham%'
update [dbo].[Site] set pfsno=209, StoreNo=676 where SiteName like '%SAINSBURYS Talbot Heath%'
update [dbo].[Site] set pfsno=154, StoreNo=887 where SiteName like '%SAINSBURYS Tamworth%'
update [dbo].[Site] set pfsno=1078, StoreNo=2078 where SiteName like '%SAINSBURYS Team Valley%'
update [dbo].[Site] set pfsno=186, StoreNo=2002 where SiteName like '%SAINSBURYS Telford%'
update [dbo].[Site] set pfsno=79, StoreNo=516 where SiteName like '%SAINSBURYS Tewkesbury%'
update [dbo].[Site] set pfsno=1324, StoreNo=2324 where SiteName like '%SAINSBURYS Thanet%'
update [dbo].[Site] set pfsno=98, StoreNo=474 where SiteName like '%SAINSBURYS Thetford%'
update [dbo].[Site] set pfsno=140, StoreNo=403 where SiteName like '%SAINSBURYS Thorley%'
update [dbo].[Site] set pfsno=211, StoreNo=732 where SiteName like '%SAINSBURYS Thorne Road%'
update [dbo].[Site] set pfsno=142, StoreNo=509 where SiteName like '%SAINSBURYS Thornhill%'
update [dbo].[Site] set pfsno=135, StoreNo=206 where SiteName like '%SAINSBURYS Tonbridge%'
update [dbo].[Site] set pfsno=198, StoreNo=668 where SiteName like '%SAINSBURYS Torquay%'
update [dbo].[Site] set pfsno=84, StoreNo=671 where SiteName like '%SAINSBURYS Truro%'
update [dbo].[Site] set pfsno=437, StoreNo=20 where SiteName like '%SAINSBURYS Tunbridge Wells%'
update [dbo].[Site] set pfsno=199, StoreNo=740 where SiteName like '%SAINSBURYS Upton%'
update [dbo].[Site] set pfsno=1295, StoreNo=2295 where SiteName like '%SAINSBURYS Wakefield%'
update [dbo].[Site] set pfsno=1244, StoreNo=2244 where SiteName like '%SAINSBURYS Wantage%'
update [dbo].[Site] set pfsno=144, StoreNo=408 where SiteName like '%SAINSBURYS Warren Heath%'
update [dbo].[Site] set pfsno=143, StoreNo=851 where SiteName like '%SAINSBURYS Warrington%'
update [dbo].[Site] set pfsno=45, StoreNo=555 where SiteName like '%SAINSBURYS Washington%'
update [dbo].[Site] set pfsno=103, StoreNo=675 where SiteName like '%SAINSBURYS Watchmoor Park%'
update [dbo].[Site] set pfsno=496, StoreNo=680 where SiteName like '%SAINSBURYS Water Lane%'
update [dbo].[Site] set pfsno=1040, StoreNo=2040 where SiteName like '%SAINSBURYS Wednesfield%'
update [dbo].[Site] set pfsno=156, StoreNo=648 where SiteName like '%SAINSBURYS Weedon%'
update [dbo].[Site] set pfsno=1610, StoreNo=610 where SiteName like '%SAINSBURYS Wellingborough%'
update [dbo].[Site] set pfsno=188, StoreNo=26 where SiteName like '%SAINSBURYS West Green%'
update [dbo].[Site] set pfsno=191, StoreNo=51 where SiteName like '%SAINSBURYS West Hove%'
update [dbo].[Site] set pfsno=195, StoreNo=57 where SiteName like '%SAINSBURYS West Park Farm%'
update [dbo].[Site] set pfsno=445, StoreNo=460 where SiteName like '%SAINSBURYS Whitechapel%'
update [dbo].[Site] set pfsno=183, StoreNo=864 where SiteName like '%SAINSBURYS Whitley Bay%'
update [dbo].[Site] set pfsno=194, StoreNo=55 where SiteName like '%SAINSBURYS Whitstable%'
update [dbo].[Site] set pfsno=842, StoreNo=854 where SiteName like '%SAINSBURYS Wigan%'
update [dbo].[Site] set pfsno=83, StoreNo=678 where SiteName like '%SAINSBURYS Winterstoke%'
update [dbo].[Site] set pfsno=471, StoreNo=735 where SiteName like '%SAINSBURYS Worksop%'
update [dbo].[Site] set pfsno=129, StoreNo=820 where SiteName like '%SAINSBURYS Worle%'
update [dbo].[Site] set pfsno=160, StoreNo=890 where SiteName like '%SAINSBURYS Wrexham%'
update [dbo].[Site] set pfsno=1110, StoreNo=9992112 where SiteName like '%SAINSBURYS Dronfield%'
update [dbo].[Site] set pfsno=1092, StoreNo=2092 where SiteName like '%SAINSBURYS Matlock%'
update [dbo].[Site] set pfsno=1023, StoreNo=2022 where SiteName like '%SAINSBURYS Attleborough%'
update [dbo].[Site] set pfsno=1180, StoreNo=2180 where SiteName like '%SAINSBURYS Dungannon%'
update [dbo].[Site] set pfsno=1517, StoreNo=4517 where SiteName like '%SAINSBURYS Lower%'
update [dbo].[Site] set pfsno=1518, StoreNo=4518 where SiteName like '%SAINSBURYS Woodlands%'
update [dbo].[Site] set pfsno=1513, StoreNo=4513 where SiteName like '%SAINSBURYS Marshalswick%'
update [dbo].[Site] set pfsno=1221, StoreNo=2221 where SiteName like '%SAINSBURYS GLOUCESTER QUAYS%'
update [dbo].[Site] set pfsno=1181, StoreNo=2181 where SiteName like '%SAINSBURYS West Belfast%'
update [dbo].[Site] set pfsno=1225, StoreNo=2225 where SiteName like '%SAINSBURYS Pontllanfraith%'
update [dbo].[Site] set pfsno=1154, StoreNo=2154 where SiteName like '%SAINSBURYS Murrayfield%'
update [dbo].[Site] set pfsno=1186, StoreNo=2186 where SiteName like '%SAINSBURYS Strathaven%'
update [dbo].[Site] set pfsno=1093, StoreNo=2093 where SiteName like '%SAINSBURYS Crystal Peaks%'
update [dbo].[Site] set pfsno=1162, StoreNo=2162 where SiteName like '%SAINSBURYS Helston%'
update [dbo].[Site] set pfsno=1850, StoreNo=850 where SiteName like '%SAINSBURYS Prestwick%'
update [dbo].[Site] set pfsno=1199, StoreNo=2199 where SiteName like '%SAINSBURYS Bishop Auckland%'
update [dbo].[Site] set pfsno=1169, StoreNo=2169 where SiteName like '%SAINSBURYS Newcastle-Under-Lyme%'
update [dbo].[Site] set pfsno=1097, StoreNo=2097 where SiteName like '%SAINSBURYS WestHoughton%'
update [dbo].[Site] set pfsno=1247, StoreNo=2247 where SiteName like '%SAINSBURYS Morecambe%'
update [dbo].[Site] set pfsno=1249, StoreNo=2249 where SiteName like '%SAINSBURYS Nairn%'
update [dbo].[Site] set pfsno=1196, StoreNo=2196 where SiteName like '%SAINSBURYS Dawlish%'
update [dbo].[Site] set pfsno=1255, StoreNo=2255 where SiteName like '%SAINSBURYS Kelso%'
update [dbo].[Site] set pfsno=1248, StoreNo=2248 where SiteName like '%SAINSBURYS Irvine%'
update [dbo].[Site] set pfsno=1274, StoreNo=2274 where SiteName like '%SAINSBURYS Pontypridd%'
update [dbo].[Site] set pfsno=1200, StoreNo=2200 where SiteName like '%SAINSBURYS Biddulph%'
update [dbo].[Site] set pfsno=1655, StoreNo=655 where SiteName like '%SAINSBURYS Taunton%'
update [dbo].[Site] set pfsno=1220, StoreNo=2220 where SiteName like '%SAINSBURYS Northfield%'
update [dbo].[Site] set pfsno=1082, StoreNo=2082 where SiteName like '%SAINSBURYS Darlington%'
update [dbo].[Site] set pfsno=1240, StoreNo=2240 where SiteName like '%SAINSBURYS Hawick%'
update [dbo].[Site] set pfsno=1272, StoreNo=2272 where SiteName like '%SAINSBURYS Whitby%'
update [dbo].[Site] set pfsno=1286, StoreNo=2286 where SiteName like '%SAINSBURYS Carlisle%'
update [dbo].[Site] set pfsno=1077, StoreNo=2077 where SiteName like '%SAINSBURYS Scunthorpe%'
update [dbo].[Site] set pfsno=1290, StoreNo=2290 where SiteName like '%SAINSBURYS Bognor Regis%'
update [dbo].[Site] set pfsno=1293, StoreNo=2293 where SiteName like '%SAINSBURYS Kings Lynn%'
update [dbo].[Site] set pfsno=1304, StoreNo=2304 where SiteName like '%SAINSBURYS Leigh New%'
update [dbo].[Site] set pfsno=1281, StoreNo=2281 where SiteName like '%SAINSBURYS Leek%'
update [dbo].[Site] set pfsno=1507, StoreNo=507 where SiteName like '%SAINSBURYS Crystal Palace%'
update [dbo].[Site] set pfsno=1239, StoreNo=2239 where SiteName like '%SAINSBURYS Colne%'
update [dbo].[Site] set pfsno=1297, StoreNo=2297 where SiteName like '%SAINSBURYS Sunderland North%'
update [dbo].[Site] set pfsno=1303, StoreNo=2303 where SiteName like '%SAINSBURYS Sedgefield%'
update [dbo].[Site] set pfsno=1524, StoreNo=4524 where SiteName like '%SAINSBURYS Horley%'
update [dbo].[Site] set pfsno=1275, StoreNo=2123 where SiteName like '%SAINSBURYS Bishops Waltham%'
update [dbo].[Site] set pfsno=1313, StoreNo=805 where SiteName like '%SAINSBURYS Dorridge%'
update [dbo].[Site] set pfsno=1309, StoreNo=2309 where SiteName like '%SAINSBURYS Edinburgh%'
update [dbo].[Site] set pfsno=1308, StoreNo=2308 where SiteName like '%SAINSBURYS Weymouth%'
update [dbo].[Site] set pfsno=1283, StoreNo=2283 where SiteName like '%SAINSBURYS Penzance%'
update [dbo].[Site] set pfsno=1314, StoreNo=2314 where SiteName like '%SAINSBURYS Leicester North%'
update [dbo].[Site] set pfsno=1267, StoreNo=2267 where SiteName like '%SAINSBURYS Bangor%'
update [dbo].[Site] set pfsno=1526, StoreNo=4526 where SiteName like '%SAINSBURYS Balsall Common%'
update [dbo].[Site] set pfsno=1525, StoreNo=4525 where SiteName like '%SAINSBURYS Balham%'
update [dbo].[Site] set pfsno=1527, StoreNo=4527 where SiteName like '%SAINSBURYS Droitwich%'
update [dbo].[Site] set pfsno=1319, StoreNo=2319 where SiteName like '%SAINSBURYS Blackpool%'
update [dbo].[Site] set pfsno=1528, StoreNo=4528 where SiteName like '%SAINSBURYS Newbury Andover Rd%'
update [dbo].[Site] set pfsno=1529, StoreNo=4529 where SiteName like '%SAINSBURYS Rawdon%'
update [dbo].[Site] set pfsno=1340, StoreNo=2340 where SiteName like '%SAINSBURYS Wolverhampton%'
update [dbo].[Site] set pfsno=1095, StoreNo=2095 where SiteName like '%SAINSBURYS Oakley%'
update [dbo].[Site] set pfsno=1252, StoreNo=2252 where SiteName like '%SAINSBURYS Portishead%'
update [dbo].[Site] set pfsno=1589, StoreNo=4589 where CatNo= 767
update [dbo].[Site] set pfsno=1289, StoreNo=2289 where SiteName like '%SAINSBURYS Waterlooville%'
update [dbo].[Site] set pfsno=1070, StoreNo=2070 where SiteName like '%SAINSBURYS Whitehouse Farm%'
update [dbo].[Site] set pfsno=1030, StoreNo=2030 where SiteName like '%SAINSBURYS Greenwich%'
update [dbo].[Site] set pfsno=1254, StoreNo=2254 where SiteName like '%SAINSBURYS Livingston%'
update [dbo].[Site] set pfsno=1158, StoreNo=2158 where SiteName like '%SAINSBURYS Dartmouth%'
update [dbo].[Site] set pfsno=1550, StoreNo=6550 where SiteName like '%SAINSBURYS Currie%'
update [dbo].[Site] set pfsno=1271, StoreNo=2271 where SiteName like '%SAINSBURYS Ely%'
update [dbo].[Site] set pfsno=131, StoreNo=18 where CatNo=15871 
update [dbo].[Site] set pfsno=466, StoreNo=867 where CatNo=9019
update [dbo].[Site] set pfsno=1038, StoreNo=2112 where CatNo=9179
update [dbo].[Site] set pfsno=91, StoreNo=59 where CatNo=9243
update [dbo].[Site] set pfsno=1051, StoreNo=2051 where CatNo=26185

