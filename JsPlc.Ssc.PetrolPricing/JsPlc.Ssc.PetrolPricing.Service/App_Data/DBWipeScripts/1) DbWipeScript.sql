﻿Delete from DailyUploadStaging;
--Truncate Table  DailyUploadStaging;
DBCC CHECKIDENT ('DailyUploadStaging', RESEED, 1);

Delete from QuarterlyUploadStaging;
--Truncate Table  QuarterlyUploadStaging;
DBCC CHECKIDENT ('QuarterlyUploadStaging', RESEED, 1);

Delete from ImportProcessError;
--Truncate Table ImportProcessError;
DBCC CHECKIDENT ('ImportProcessError', RESEED, 1);

Delete from DailyPrice;
--Truncate Table  DailyPrice;
DBCC CHECKIDENT ('DailyPrice', RESEED, 1);

Delete from SitePrice;
--Truncate Table  SitePrice;
DBCC CHECKIDENT ('SitePrice', RESEED, 1);

Delete from FileUpload; 
--ALTER TABLE FileUpload NOCHECK CONSTRAINT ALL;
--Truncate Table FileUpload;
--ALTER TABLE FileUpload WITH CHECK CHECK CONSTRAINT ALL;
DBCC CHECKIDENT ('FileUpload', RESEED, 1);

Delete from SiteEmail;
--Truncate Table  SiteEmail;
DBCC CHECKIDENT ('SiteEmail', RESEED, 1);

Delete from SiteToCompetitor;
--Truncate Table  SiteToCompetitor;
DBCC CHECKIDENT ('SiteToCompetitor', RESEED, 1);

Delete from Site;
--Truncate Table  Site;
DBCC CHECKIDENT ('Site', RESEED, 1);

-- Following table IDs inserted by Seed, so no RESEED 0 needed

Delete from FuelType;
--Truncate Table  FuelType;

Delete from UploadType;
--Truncate Table  UploadType;

Delete from ImportProcessStatus;
--Truncate Table  ImportProcessStatus;

Delete from AppConfigSettings;
--Truncate Table  AppConfigSettings;