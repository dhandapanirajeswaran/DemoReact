using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryInitializer : DropCreateDatabaseIfModelChanges<RepositoryContext>
    {
        protected override void Seed(RepositoryContext context)
        {
            SeedRepository(context);
            base.Seed(context);
        }

        // This is refactored out so it can be called separately as well
        public static void SeedRepository(RepositoryContext context)
        {
            var appConfigSettings = new List<AppConfigSettings>
            {
                // \\A-cotufps01-p.bc.jsplc.net\userdatashare0001\Parveen.Kumar\TestPetrolUpload

                new AppConfigSettings{Id = 1, SettingKey = "Uploadpath", 
                    SettingValue = @"\\feltfps0003\gengrpshare0037\Scrum Teams\000000 - Projects\122000 - Petrol Pricing\TestFileUpload"},
                new AppConfigSettings{Id = (int)SettingsKeys.SomeOtherVal, SettingKey = SettingsKeys.SomeOtherVal.ToString(), 
                    SettingValue = ""}

                //new AppConfigSettings{Id = 1, SettingKey = "Uploadpath", 
                //SettingValue = ""},
            };
            appConfigSettings.ForEach(a => context.AppConfigSettings.Add(a));
            context.SaveChanges();

            // # 1=Super, 2=Unleaded, 5=Super Dis, 6=Std Dis, 7=LPG
            var fuelTypes = new List<FuelType>{
                new FuelType{Id=1, FuelTypeName = "Super Unleaded"},
                new FuelType{Id=2, FuelTypeName = "Unleaded"},
                new FuelType{Id=5, FuelTypeName = "Super Diesel"},
                new FuelType{Id=6, FuelTypeName = "Diesel"},
                new FuelType{Id=7, FuelTypeName = "LPG"},
            };

            fuelTypes.ForEach(f => context.FuelType.Add(f));
            context.SaveChanges();

            // # Uploaded,Processing,Success,Failed (given gaps so if we want to introduce other status in between
            var importProcessStatuses = new List<ImportProcessStatus>{
                new ImportProcessStatus{Id=1, Status = "Uploaded"},
                new ImportProcessStatus{Id=5, Status = "Processing"},
                new ImportProcessStatus{Id=10, Status = "Success"},
                new ImportProcessStatus{Id=15, Status = "Failed"},
            };

            importProcessStatuses.ForEach(ips => context.ImportProcessStatus.Add(ips));
            context.SaveChanges();

            // # Daily, Quarterly
            var uploadTypes = new List<UploadType>{
                new UploadType{Id=1, UploadTypeName = "Daily Price Data"},
                new UploadType{Id=2, UploadTypeName = "Quarterly Site Data"},
            };

            uploadTypes.ForEach(ut => context.UploadType.Add(ut));
            context.SaveChanges();

            var sites = new List<Site>{
                new Site{SiteName = "SAINSBURYS HENDON", Town = "London", 
                    Address = "HYDE ESTATE ROAD", Suburb = "HENDON", PostCode = "NW9 6JX", Company = "J SAINSBURY PLC", 
                    Ownership = "Hypermarket", CatNo = 100, Brand = "SAINSBURYS", IsSainsburysSite = true, IsActive = true}, 
                   // Store No = 637, PFS = 66 (can add while editing)

                new Site{SiteName = "SAINSBURYS ALPERTON", Town = "WEMBLEY", 
                    Address = "EALING ROAD", Suburb = "ALPERTON", PostCode = "HA0 1PF", Company = "J SAINSBURY PLC", 
                    Ownership = "Hypermarket", CatNo = 1334, Brand = "SAINSBURYS", IsSainsburysSite = true, IsActive = true, StoreNo = 646},
                   // PFS = 196 (can add/amend while editing)

                new Site{SiteName = "ASDA COLINDALE AUTOMAT", Town = "London", 
                    Address = "CAPITOL WAY", Suburb = "COLINDALE", PostCode = "NW9 0EW", Company = "ASDA STORES PLC", 
                    Ownership = "Hypermarket", CatNo = 26054, Brand = "ASDA", IsSainsburysSite = false, IsActive = true},

                new Site{SiteName = "TESCO HOOVER BUILDING", Town = "GREENFORD", 
                    Address = "WESTERN AVENUE", Suburb = "PERIVALE", PostCode = "UB6 8DW", Company = "TESCO STORES LTD", 
                    Ownership = "Hypermarket", CatNo = 1336, Brand = "TESCO", IsSainsburysSite = false, IsActive = true},
            };

            //sites.ForEach(c => context.Sites.Add(c));
            sites.ForEach(s => context.Sites.AddOrUpdate(p => p.SiteName, s));
            context.SaveChanges();

            var siteEmails = new List<SiteEmail>
            {
                new SiteEmail
                {
                    EmailAddress = "Sainsburys.hendon1@sainsburys.co.uk",
                    SiteId = sites.Single(i => i.SiteName == "SAINSBURYS HENDON").Id
                },
                new SiteEmail
                {
                    EmailAddress = "Sainsburys.hendon2@sainsburys.co.uk",
                    SiteId = sites.Single(i => i.SiteName == "SAINSBURYS HENDON").Id
                }
                // no emails assigned to Sainsburys Alperton for now
            };
            siteEmails.ForEach(s => context.SiteEmails.AddOrUpdate(p => p.EmailAddress, s));
            context.SaveChanges();
        }
    }
}
