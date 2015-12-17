--            //  100 -> 26054, 1336, 99, 1751
--            //  1334 -> 26054, 1336
-- 2 = Unleaded, 6 = Diesel
-- NOTE: Divide price by 1000 to get pound value, OR by 10 to ge pence value
--Use PetrolPricingRepository
Declare @firstFileId int
Set @firstFileId = (Select Top 1 Id from FileUpload Order By Id Asc)

INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
	Values ( @firstFileId,	26054,	2,	0,	'2015-11-30',	1069)
INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	26054,	6,	0,	'2015-11-30',	1089)

INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	1336 ,	2,	0,	'2015-11-30',	1059)
INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	1336 ,	6,	0,	'2015-11-30',	1089)

INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	99   ,	2,	0,	'2015-11-30',	1059)
INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	99   ,	6,	0,	'2015-11-30',	1089)

INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	1751 ,	2,	0,	'2015-11-30',	1109)
INSERT INTO [dbo].[DailyPrice] ([DailyUploadId],[CatNo],[FuelTypeId],[AllStarMerchantNo],[DateOfPrice],[ModalPrice])
    Values ( @firstFileId,	1751 ,	6,	0,	'2015-11-30',	1119)


