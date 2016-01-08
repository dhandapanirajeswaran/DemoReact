using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using EntityFramework.Utilities;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using MoreLinq;
using EntityState = System.Data.Entity.EntityState;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryInitializer : DropCreateDatabaseIfModelChanges<RepositoryContext>
    {
        protected override void Seed(RepositoryContext context)
        {
            SeedRepository(context);
            base.Seed(context);
        }

        /// <summary>
        /// Deletes and reloads AppSettings keys/values
        /// </summary>
        /// <param name="context"></param>
        public static void ReInitConfigKeys(RepositoryContext context)
        {
            context.AppConfigSettings.ForEach(x => context.Entry(x).State = EntityState.Deleted);
            context.SaveChanges();
            DbInitConfigKeys(context);
        }

        /// <summary>
        /// Deletes and recreates sprocs
        /// </summary>
        /// <param name="context"></param>
        public static void ReInitSprocs(RepositoryContext context)
        {
            RunDbScripts(context, ScriptFolderType.DropCreateSprocs);
        }

        /// <summary>
        /// Deletes and recreates indexes
        /// </summary>
        /// <param name="context"></param>
        public static void ReInitIndexes(RepositoryContext context)
        {
            RunDbScripts(context, ScriptFolderType.DropCreateIndexes);
        }

        // This is refactored out so it can be called separately as well
        public static void SeedRepository(RepositoryContext context)
        {
            RunDbScripts(context, ScriptFolderType.DbWipeScripts);

            DbInitConfigKeys(context);

            DbInitFuelTypes(context);

            DbInitImportProcessStatus(context);

            DbInitUploadTypes(context);

            // TODO - Comment out after Site Imports start working
            SetupDummyData(context);

            ReInitSprocs(context);

            RunDbScripts(context, ScriptFolderType.PostSeedScripts);
        }

        private static void SetupDummyData(RepositoryContext context)
        {
            DbInitDummyProjectData(context);
            RunDbScripts(context, ScriptFolderType.DummyDataScripts);
        }

        private static void DbInitDummyProjectData(RepositoryContext context)
        {
            // TODO - Comment out after Site Imports start working
            var sites = new List<Site>{
                new Site{SiteName = "SAINSBURYS HENDON", Town = "London", 
                    Address = "HYDE ESTATE ROAD", Suburb = "HENDON", PostCode = "NW9 6JX", Company = "J SAINSBURY PLC", 
                    Ownership = "Hypermarket", CatNo = 100, Brand = "SAINSBURYS", IsSainsburysSite = true, IsActive = true}, 
                   // PFS=66, Cat=100, Store= 637

                new Site{SiteName = "SAINSBURYS ALPERTON", Town = "WEMBLEY", 
                    Address = "EALING ROAD", Suburb = "ALPERTON", PostCode = "HA0 1PF", Company = "J SAINSBURY PLC", 
                    Ownership = "Hypermarket", CatNo = 1334, Brand = "SAINSBURYS", IsSainsburysSite = true, IsActive = true, StoreNo = 646},
                   // PFS=196, Cat=1334, Store=646

                new Site{SiteName = "SAINSBURYS FARLINGTON", Town = "PORTSMOUTH", 
                    Address = "FITZHERBERT ROAD", Suburb = "DRAYTON", PostCode = "PO6 1RR", Company = "J SAINSBURY PLC", 
                    Ownership = "Hypermarket", CatNo = 9144, Brand = "SAINSBURYS", IsSainsburysSite = true, IsActive = true, PfsNo = 81},
                    // PFS=81	Cat=9144	Site=672

                // ### ### ### ### ### ### 
                // ### COMPETITORS
                // ### ### ### ### ### ### 
                new Site{SiteName = "TESCO HOOVER BUILDING", Town = "GREENFORD", 
                    Address = "WESTERN AVENUE", Suburb = "PERIVALE", PostCode = "UB6 8DW", Company = "TESCO STORES LTD", 
                    Ownership = "Hypermarket", CatNo = 1336, Brand = "TESCO", IsSainsburysSite = false, IsActive = true},

                new Site{SiteName = "ASDA WATERLOOVILLE AUTOMAT", Town = "WATERLOOVILLE", 
                    Address = "MAUREPASS WAY", Suburb = "", PostCode = "PO7 7XR", Company = "ASDA STORES PLC", 
                    Ownership = "Hypermarket", CatNo = 26053, Brand = "ASDA", IsSainsburysSite = false, IsActive = true},

                new Site{SiteName = "ASDA COLINDALE AUTOMAT", Town = "London", 
                    Address = "CAPITOL WAY", Suburb = "COLINDALE", PostCode = "NW9 0EW", Company = "ASDA STORES PLC", 
                    Ownership = "Hypermarket", CatNo = 26054, Brand = "ASDA", IsSainsburysSite = false, IsActive = true},

                new Site{SiteName = "PARK WELSH HARP SERVICE STATION", Town = "London",
                    Address = "THE BROADWAY", Suburb = "HENDON", PostCode = "NW9 7DN", Company = "PARK GARAGE GROUP", 
                    Ownership = "DEALER", CatNo = 99, Brand = "BP", IsSainsburysSite = false, IsActive = true},

                new Site{SiteName = "CO-OP HENDON WAY", Town = "London",
                    Address = "WATFORD WAY", Suburb = "HENDON", PostCode = "NW4 3AQ", Company = "CO-OP GROUP", 
                    Ownership = "DEALER", CatNo = 1751, Brand = "TEXACO", IsSainsburysSite = false, IsActive = true},
            };

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

            // TODO - Comment out after Daily Price Imports start working
            var siteToCompetitor = new List<SiteToCompetitor>
            {
                // 100 - 26054, 1, 1.42, 6.66
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 100).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 26054).Id,
                    Rank = 1,
                    Distance = 1.42f,
                    DriveTime = 6.66f
                },
                // 100 - 1336, 16, 6.50, 21.35
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 100).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 1336).Id,
                    Rank = 16,
                    Distance = 6.50f,
                    DriveTime = 21.35f
                },
                // 100 - 99, 1, 0.34, 1.54
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 100).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 99).Id,
                    Rank = 1,
                    Distance = 0.34f,
                    DriveTime = 1.54f
                },
                // 100 - 1751, 3, 1.27, 6.25
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 100).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 1751).Id,
                    Rank = 3,
                    Distance = 1.27f,
                    DriveTime = 6.25f
                },
                // 1334 - 1336, 1, 1.52, 8.01
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 1334).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 1336).Id,
                    Rank = 1,
                    Distance = 1.52f,
                    DriveTime = 8.01f
                },
                // 1334 - 26054, 16, 5.33, 20.53
                new SiteToCompetitor
                {
                    SiteId = sites.Single(i => i.CatNo == 1334).Id,
                    CompetitorId = sites.Single(i => i.CatNo == 26054).Id,
                    Rank = 16,
                    Distance = 5.33f,
                    DriveTime = 20.53f
                },
            };
            siteToCompetitor.ForEach(s => context.SiteToCompetitors.AddOrUpdate(p => p.SiteId, s));
            context.SaveChanges();

            var importProcessStatuses = context.ImportProcessStatus.ToList();
            var uploadTypes = context.UploadType.ToList();

            // Dummy FileUpload to link with Daily Prices, Dated today, TODO comment out once Daily Price Imports start working
            // Assume Succes-ful import as we have hardCoded dailyPrices for this hardcoded Upload
            var fileuploads = new List<FileUpload>
            {
                new FileUpload
                {
                    OriginalFileName = "Daily Price file.txt",
                    Status = importProcessStatuses.Single(x => x.Status == "Success"),
                    StoredFileName = @"\\UNC\Daily Price file.txt",
                    UploadDateTime = DateTime.Parse("2015-11-30"),
                    UploadType = uploadTypes.Single(x => x.UploadTypeName == "Daily Price Data"),
                    UploadedBy = "RepositoryInitializer@sainsburys.co.uk"
                }
            };
            fileuploads.ForEach(f => context.FileUploads.Add(f));
            context.SaveChanges();
        }

        private static void DbInitUploadTypes(RepositoryContext context)
        {
            // # Daily, Quarterly
            var uploadTypes = new List<UploadType>{
                new UploadType{Id=1, UploadTypeName = "Daily Price Data"},
                new UploadType{Id=2, UploadTypeName = "Quarterly Site Data"},
            };

            uploadTypes.ForEach(ut => context.UploadType.Add(ut));
            context.SaveChanges();
        }

        private static void DbInitImportProcessStatus(RepositoryContext context)
        {
            // # Uploaded,Processing,Success,Failed (given gaps so if we want to introduce other status in between
            var importProcessStatuses = new List<ImportProcessStatus>{
                new ImportProcessStatus{Id=1, Status = "Uploaded"}, // only in FileSystem, not in table
                new ImportProcessStatus{Id=5, Status = "Processing"}, // Importing to DP Table
                new ImportProcessStatus{Id=10, Status = "Success"}, // Imported to DP, Usable for recalc
                new ImportProcessStatus{Id=11, Status = "Calculating"}, // Populating SitePrice using DP
                new ImportProcessStatus{Id=12, Status = "CalcFailed"}, // Populating SP failed, Usable for recalc
                new ImportProcessStatus{Id=15, Status = "Failed"}, // Importing to DP failed
                new ImportProcessStatus{Id=16, Status = "ImportAborted"}, // Import or Calc timeout, Usable for recalc
                new ImportProcessStatus{Id=17, Status = "CalcAborted"}, // Import or Calc timeout, Usable for recalc
            };

            importProcessStatuses.ForEach(ips => context.ImportProcessStatus.Add(ips));
            context.SaveChanges();
        }

        private static void DbInitFuelTypes(RepositoryContext context)
        {
            // # 1=Super, 2=Unleaded, 6=Std Dis, Only these 3 are used 
            // Unused 5=Super Dis, 7=LPG
            var fuelTypes = new List<FuelType>{
                new FuelType{Id=1, FuelTypeName = "Super Unleaded"},
                new FuelType{Id=2, FuelTypeName = "Unleaded"},
                // TODO update when query clarified with Izzy. So far we might see these fuelCodes in Catalist so included it, so that imports carry on
                new FuelType{Id=3, FuelTypeName = "Unknown1"}, // Unused
                new FuelType{Id=4, FuelTypeName = "Unknown2"}, // Unused
                new FuelType{Id=5, FuelTypeName = "Super Diesel"}, // Unused
                new FuelType{Id=6, FuelTypeName = "Diesel"},
                new FuelType{Id=7, FuelTypeName = "LPG"}, //Unused
            };

            fuelTypes.ForEach(f => context.FuelType.Add(f));
            context.SaveChanges();
        }

        private static void DbInitConfigKeys(RepositoryContext context)
        {
            var configSettingKeys = ConfigurationManager.AppSettings.AllKeys;

            int id = 1;
            var appConfigSettings = new List<AppConfigSettings>();
            foreach (var key in configSettingKeys)
            {
                appConfigSettings.Add(new AppConfigSettings
                {
                    Id = id,
                    SettingKey = key,
                    SettingValue = ConfigurationManager.AppSettings[key]
                });
                id += 1;
            }
            appConfigSettings.ForEach(a => context.AppConfigSettings.Add(a));
            context.SaveChanges();
        }

        /// <summary>
        /// Runs all sql scripts stored in the specified folder.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scriptFolderType"></param>
        private static void RunDbScripts(RepositoryContext context, ScriptFolderType scriptFolderType)
        {
            var appDataFolderPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            
            var scriptFolderPath = Path.Combine(appDataFolderPath, scriptFolderType.ToString());
            foreach (string filePath in Directory.EnumerateFiles(scriptFolderPath, "*.sql").OrderBy(x => x))
            {
                Debug.WriteLine("Script found:" + filePath);
                var scriptPath = Path.Combine(appDataFolderPath, filePath);

                string sqlScript = File.ReadAllText(scriptPath);
                var conn = context.Database.Connection;
                try
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sqlScript;
                    cmd.CommandType = CommandType.Text;

                    //var x = context.Database.ExecuteSqlCommand(sqlScript);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Script failed:" + sqlScript + "---" + ex.Message + ex.StackTrace, ex);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }

    public enum ScriptFolderType
    {
        DbWipeScripts =1,
        DummyDataScripts =2,
        PostSeedScripts =3,
        DropCreateSprocs = 4,
        DropCreateIndexes = 5
    }

}
