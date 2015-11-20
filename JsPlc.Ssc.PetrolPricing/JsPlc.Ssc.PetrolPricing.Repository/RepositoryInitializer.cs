﻿using System;
using System.Data.Entity;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryInitializer : DropCreateDatabaseIfModelChanges<RepositoryContext>
    {
        // TODO Somehow see doesnt run from portal
        protected override void Seed(RepositoryContext context)
        {
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
            
            var sites=new List<Site>{
                new Site{Id=1, SiteName = "JS Dummy Site1", Town = "Coventry"},
            };

            sites.ForEach(c => context.Sites.Add(c));
            context.SaveChanges();
            
            base.Seed(context);
        }
    }
}
