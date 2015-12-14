Delete from DailyUploadStaging;
Truncate Table  DailyUploadStaging;

Delete from QuarterlyUploadStaging;
Truncate Table  QuarterlyUploadStaging;

Delete from ImportProcessError;
Truncate Table ImportProcessError;

Delete from DailyPrice;
Truncate Table  DailyPrice;
DBCC CHECKIDENT ('DailyPrice', RESEED, 1);

Delete from SitePrice;
Truncate Table  SitePrice;
DBCC CHECKIDENT ('SitePrice', RESEED, 1);

Delete from FileUpload; 
--Truncate Table FileUpload;
DBCC CHECKIDENT ('FileUpload', RESEED, 1);

Delete from SiteEmail;
--Truncate Table  SiteEmail;
DBCC CHECKIDENT ('SiteEmail', RESEED, 1);

Delete from SiteToCompetitor;
--Truncate Table  SiteToCompetitor;

Delete from Site;
--Truncate Table  Site;

Delete from FuelType;
--Truncate Table  FuelType;

Delete from UploadType;
--Truncate Table  UploadType;

Delete from ImportProcessStatus;
--Truncate Table  ImportProcessStatus;

Delete from AppConfigSettings;
--Truncate Table  AppConfigSettings;

