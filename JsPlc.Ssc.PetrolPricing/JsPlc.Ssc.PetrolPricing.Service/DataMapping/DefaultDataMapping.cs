using AutoMapper;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.ExtensionMethods;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using System;

namespace JsPlc.Ssc.PetrolPricing.Service.DataMapping
{
    public class DefaultDataMapping : IAutoMapperInitialise
    {
        /// <summary>
        /// Configures the mapping between the <see cref="DataTableArchiveSearchRequestViewModel"/>
        /// and its counterparts.
        /// </summary>
        public void Initialise()
        {
            Mapper.CreateMap<OverridePricePostViewModel, SitePrice>()
                .ForMember(co => co.FuelTypeId, mo => mo.MapFrom(p => p.FuelTypeId))
                .ForMember(co => co.SiteId, mo => mo.MapFrom(p => p.SiteId))
                .ForMember(co => co.OverriddenPrice, i => i.MapFrom(p => Convert.ToInt32(p.OverridePrice * 10)))
                .ForMember(co => co.Id, mo => mo.Ignore())
                .ForMember(co => co.JsSite, mo => mo.Ignore())
                .ForMember(co => co.FuelType, mo => mo.Ignore())
                .ForMember(co => co.DateOfCalc, mo => mo.Ignore())
                .ForMember(co => co.DateOfPrice, mo => mo.Ignore())
                .ForMember(co => co.UploadId, mo => mo.Ignore())
                .ForMember(co => co.EffDate, mo => mo.Ignore())
                .ForMember(co => co.SuggestedPrice, mo => mo.Ignore())
                .ForMember(co => co.CompetitorId, mo => mo.Ignore())
                .ForMember(co => co.Markup, mo => mo.Ignore())
                .ForMember(co => co.IsTrailPrice, mo => mo.Ignore());

            Mapper.CreateMap<SiteEmailViewModel, SiteEmail>()
                .ForMember(co => co.Site, mo => mo.Ignore());

            Mapper.CreateMap<SiteViewModel, Site>()
                .ForMember(co => co.Address, mo => mo.MapFrom(p => p.Address))
                .ForMember(co => co.Brand, mo => mo.MapFrom(p => p.Brand))
                .ForMember(co => co.CatNo, i => i.MapFrom(p => p.CatNo))
                .ForMember(co => co.Company, i => i.MapFrom(p => p.Company))
                .ForMember(co => co.Competitors, mo => mo.Ignore()) //ignore
                .ForMember(co => co.Emails, i => i.MapFrom(p => p.Emails)) //list map
                .ForMember(co => co.Id, i => i.MapFrom(p => p.Id))
                .ForMember(co => co.IsActive, i => i.MapFrom(p => p.IsActive))
                .ForMember(co => co.IsSainsburysSite, i => i.MapFrom(p => p.IsSainsburysSite))
                .ForMember(co => co.Ownership, i => i.MapFrom(p => p.Ownership))
                .ForMember(co => co.PfsNo, i => i.MapFrom(p => p.PfsNo))
                .ForMember(co => co.PostCode, i => i.MapFrom(p => p.PostCode))
                .ForMember(co => co.Prices, mo => mo.Ignore()) //ignore
                .ForMember(co => co.SiteName, i => i.MapFrom(p => p.SiteName))
                .ForMember(co => co.StoreNo, i => i.MapFrom(p => p.StoreNo))
                .ForMember(co => co.Suburb, i => i.MapFrom(p => p.Suburb))
                .ForMember(co => co.Town, i => i.MapFrom(p => p.Town))
                .ForMember(co => co.TrailPriceCompetitorId, i => i.MapFrom(p => p.TrailPriceCompetitorId))
                .ForMember(co => co.CompetitorPriceOffset, i => i.MapFrom(p => p.CompetitorPriceOffset))
                .ForMember(co => co.hasNotes, mo => mo.Ignore())
                .ForMember(co => co.Notes, i => i.MapFrom(p => p.Notes))
                .ForMember(co => co.HasNearbyUnleadedGrocers, i=> i.MapFrom(p => p.HasNearbyUnleadedGrocers))
                .ForMember(co => co.HasNearbyUnleadedGrocersPriceData, i=> i.MapFrom(p => p.HasNearbyUnleadedGrocersPriceData))
                .ForMember(co => co.HasNearbyDieselGrocers, i=> i.MapFrom(p => p.HasNearbyDieselGrocers))
                .ForMember(co => co.HasNearbyDieselGrocersPriceData, i => i.MapFrom(p => p.HasNearbyDieselGrocersPriceData))
                .ForMember(co => co.HasNearbySuperUnleadedGrocers, i => i.MapFrom(p => p.HasNearbySuperUnleadedGrocers))
                .ForMember(co => co.HasNearbySuperUnleadedGrocersPriceData, i => i.MapFrom(p => p.HasNearbySuperUnleadedGrocersPriceData));

            Mapper.CreateMap<FileUpload, FileUploadViewModel>()
                .ForMember(fu => fu.Id, mo => mo.MapFrom(p => p.Id))
                .ForMember(fu => fu.OriginalFileName, mo => mo.MapFrom(p => p.OriginalFileName))
                .ForMember(fu => fu.StoredFileName, mo => mo.MapFrom(p => p.StoredFileName))
                .ForMember(fu => fu.UploadTypeId, mo => mo.Ignore())
                .ForMember(fu => fu.UploadType, mo => mo.MapFrom(p => p.UploadType))
                .ForMember(fu => fu.UploadDateTime, mo => mo.MapFrom(p => p.UploadDateTime))
                .ForMember(fu => fu.StatusId, mo => mo.MapFrom(p => p.StatusId))
                .ForMember(fu => fu.Status, mo => mo.MapFrom(p => p.Status))
                .ForMember(fu => fu.UploadedBy, mo => mo.MapFrom(p => p.UploadedBy))
                .ForMember(fu => fu.FileExists, mo => mo.MapFrom(p => p.FileExists))
                .ForMember(fu => fu.ImportProcessErrors, mo => mo.Ignore())
                .ForMember(fu => fu.IsMostRecentForDate, mo => mo.Ignore())
                .ForMember(fu => fu.IsForDifferentDay, mo => mo.Ignore());

            //
            // SystemSettings >> SystemSettingsViewModel
            //
            Mapper.CreateMap<SystemSettings, SystemSettingsViewModel>()
                .ForMember(dst => dst.Id, src => src.MapFrom(p => p.Id))

                .ForMember(dst => dst.Status, src => src.Ignore())

                .ForMember(dst => dst.MinUnleadedPrice, src => src.MapFrom(p => p.MinUnleadedPrice.ToActualPrice()))
                .ForMember(dst => dst.MaxUnleadedPrice, src => src.MapFrom(p => p.MaxUnleadedPrice.ToActualPrice()))
                .ForMember(dst => dst.MinDieselPrice, src => src.MapFrom(p => p.MinDieselPrice.ToActualPrice()))
                .ForMember(dst => dst.MaxDieselPrice, src => src.MapFrom(p => p.MaxDieselPrice.ToActualPrice()))
                .ForMember(dst => dst.MinSuperUnleadedPrice, src => src.MapFrom(p => p.MinSuperUnleadedPrice.ToActualPrice()))
                .ForMember(dst => dst.MaxSuperUnleadedPrice, src => src.MapFrom(p => p.MaxSuperUnleadedPrice.ToActualPrice()))
                .ForMember(dst => dst.MinUnleadedPriceChange, src => src.MapFrom(p => p.MinUnleadedPriceChange.ToActualPrice()))
                .ForMember(dst => dst.MaxUnleadedPriceChange, src => src.MapFrom(p => p.MaxUnleadedPriceChange.ToActualPrice()))
                .ForMember(dst => dst.MinDieselPriceChange, src => src.MapFrom(p => p.MinDieselPriceChange.ToActualPrice()))
                .ForMember(dst => dst.MaxDieselPriceChange, src => src.MapFrom(p => p.MaxDieselPriceChange.ToActualPrice()))
                .ForMember(dst => dst.MinSuperUnleadedPriceChange, src => src.MapFrom(p => p.MinSuperUnleadedPriceChange.ToActualPrice()))
                .ForMember(dst => dst.MaxSuperUnleadedPriceChange, src => src.MapFrom(p => p.MaxSuperUnleadedPriceChange.ToActualPrice()))

                .ForMember(dst => dst.MaxGrocerDriveTimeMinutes, src => src.MapFrom(p => p.MaxGrocerDriveTimeMinutes))

                .ForMember(dst => dst.PriceChangeVarianceThreshold, src => src.MapFrom(p => p.PriceChangeVarianceThreshold.ToActualPrice()))
                .ForMember(dst => dst.SuperUnleadedMarkupPrice, src => src.MapFrom(p => p.SuperUnleadedMarkupPrice.ToActualPrice()))
                .ForMember(dst => dst.DecimalRounding, src => src.MapFrom(p => p.DecimalRounding))
                .ForMember(dst => dst.EnableSiteEmails, src => src.MapFrom(p => p.EnableSiteEmails))
                .ForMember(dst => dst.SiteEmailTestAddresses, src => src.MapFrom(p => p.SiteEmailTestAddresses));

            //
            // SystemSettingsViewModel >> SystemSettings
            //
            Mapper.CreateMap<SystemSettingsViewModel, SystemSettings>()
                .ForMember(dst => dst.Id, src => src.MapFrom(p => p.Id))

                .ForMember(dst => dst.DataCleanseFilesAfterDays, src => src.Ignore())
                .ForMember(dst => dst.LastDataCleanseFilesOn, src => src.Ignore())

                .ForMember(dst => dst.MinUnleadedPrice, src => src.MapFrom(p => p.MinUnleadedPrice.ToModalPrice()))
                .ForMember(dst => dst.MaxUnleadedPrice, src => src.MapFrom(p => p.MaxUnleadedPrice.ToModalPrice()))
                .ForMember(dst => dst.MinDieselPrice, src => src.MapFrom(p => p.MinDieselPrice.ToModalPrice()))
                .ForMember(dst => dst.MaxDieselPrice, src => src.MapFrom(p => p.MaxDieselPrice.ToModalPrice()))
                .ForMember(dst => dst.MinSuperUnleadedPrice, src => src.MapFrom(p => p.MinSuperUnleadedPrice.ToModalPrice()))
                .ForMember(dst => dst.MaxSuperUnleadedPrice, src => src.MapFrom(p => p.MaxSuperUnleadedPrice.ToModalPrice()))
                .ForMember(dst => dst.MinUnleadedPriceChange, src => src.MapFrom(p => p.MinUnleadedPriceChange.ToModalPrice()))
                .ForMember(dst => dst.MaxUnleadedPriceChange, src => src.MapFrom(p => p.MaxUnleadedPriceChange.ToModalPrice()))
                .ForMember(dst => dst.MinDieselPriceChange, src => src.MapFrom(p => p.MinDieselPriceChange.ToModalPrice()))
                .ForMember(dst => dst.MaxDieselPriceChange, src => src.MapFrom(p => p.MaxDieselPriceChange.ToModalPrice()))
                .ForMember(dst => dst.MinSuperUnleadedPriceChange, src => src.MapFrom(p => p.MinSuperUnleadedPriceChange.ToModalPrice()))
                .ForMember(dst => dst.MaxSuperUnleadedPriceChange, src => src.MapFrom(p => p.MaxSuperUnleadedPriceChange.ToModalPrice()))

                .ForMember(dst => dst.MaxGrocerDriveTimeMinutes, src => src.MapFrom(p => p.MaxGrocerDriveTimeMinutes))

                .ForMember(dst => dst.PriceChangeVarianceThreshold, src => src.MapFrom(p => p.PriceChangeVarianceThreshold.ToModalPrice()))
                .ForMember(dst => dst.SuperUnleadedMarkupPrice, src => src.MapFrom(p => p.SuperUnleadedMarkupPrice.ToModalPrice()))
                .ForMember(dst => dst.DecimalRounding, src => src.MapFrom(p => p.DecimalRounding))
                .ForMember(dst => dst.EnableSiteEmails, src => src.MapFrom(p => p.EnableSiteEmails))
                .ForMember(dst => dst.SiteEmailTestAddresses, src => src.MapFrom(p => p.SiteEmailTestAddresses));


            Mapper.CreateMap<EmailTemplate, EmailTemplateViewModel>()
                .ForMember(dst => dst.EmailTemplateId, src => src.MapFrom(p => p.EmailTemplateId))
                .ForMember(dst => dst.IsDefault, src => src.MapFrom(p => p.IsDefault))
                .ForMember(dst => dst.TemplateName, src => src.MapFrom(p => p.TemplateName))
                .ForMember(dst => dst.SubjectLine, src => src.MapFrom(p => p.SubjectLine))
                .ForMember(dst => dst.PPUserId, src => src.MapFrom(p => p.PPUserId))
                .ForMember(dst => dst.PPUserEmail, src => src.Ignore())
                .ForMember(dst => dst.EmailBody, src => src.MapFrom(p => p.EmailBody));

            Mapper.CreateMap<EmailTemplateViewModel, EmailTemplate>()
                .ForMember(dst => dst.EmailTemplateId, src => src.MapFrom(p => p.EmailTemplateId))
                .ForMember(dst => dst.IsDefault, src => src.MapFrom(p => p.IsDefault))
                .ForMember(dst => dst.TemplateName, src => src.MapFrom(p => p.TemplateName))
                .ForMember(dst => dst.SubjectLine, src => src.MapFrom(p => p.SubjectLine))
                .ForMember(dst => dst.PPUserId, src => src.MapFrom(p => p.PPUserId))
                .ForMember(dst => dst.EmailBody, src => src.MapFrom(p => p.EmailBody));

            Mapper.CreateMap<EmailTemplateName, EmailTemplateNameViewModel>()
                .ForMember(dst => dst.EmailTemplateId, src => src.MapFrom(p => p.EmailTemplateId))
                .ForMember(dst => dst.TemplateName, src => src.MapFrom(p => p.TemplateName))
                .ForMember(dst => dst.IsDefault, src => src.MapFrom(p => p.IsDefault))
                .ForMember(dst => dst.SubjectLine, src => src.MapFrom(p => p.SubjectLine));

            Mapper.CreateMap<EmailTemplateNameViewModel, EmailTemplateName>()
                .ForMember(dst => dst.EmailTemplateId, src => src.MapFrom(p => p.EmailTemplateId))
                .ForMember(dst => dst.TemplateName, src => src.MapFrom(p => p.TemplateName))
                .ForMember(dst => dst.IsDefault, src => src.MapFrom(p => p.IsDefault))
                .ForMember(dst => dst.SubjectLine, src => src.MapFrom(p => p.SubjectLine));

            //
            // DriveTimeMarkup -> DriveTimeMarkupViewModel
            //
            Mapper.CreateMap<DriveTimeMarkup, DriveTimeMarkupViewModel>()
                .ForMember(dst => dst.Id, src => src.MapFrom(p => p.Id))
                .ForMember(dst => dst.FuelTypeId, src => src.MapFrom(p => p.FuelTypeId))
                .ForMember(dst => dst.DriveTime, src => src.MapFrom(p => p.DriveTime))
                .ForMember(dst => dst.Markup, src => src.MapFrom(p => p.Markup))
                .ForMember(dst => dst.MaxDriveTime, src => src.MapFrom(p => p.MaxDriveTime))
                .ForMember(dst => dst.IsFirst, src => src.MapFrom(p => p.IsFirst))
                .ForMember(dst => dst.IsLast, src => src.MapFrom(p => p.IsLast));

            //
            // DriveTimeMarkupViewModel -> DriveTimeMarkup
            //
            Mapper.CreateMap<DriveTimeMarkupViewModel, DriveTimeMarkup>()
                .ForMember(dst => dst.Id, src => src.MapFrom(p => p.Id))
                .ForMember(dst => dst.FuelTypeId, src => src.MapFrom(p => p.FuelTypeId))
                .ForMember(dst => dst.DriveTime, src => src.MapFrom(p => p.DriveTime))
                .ForMember(dst => dst.Markup, src => src.MapFrom(p => p.Markup))

                // ignore calculated properties
                .ForMember(dst => dst.MaxDriveTime, src => src.Ignore())
                .ForMember(dst => dst.IsFirst, src => src.Ignore())
                .ForMember(dst => dst.IsLast, src => src.Ignore());
        }
    }
}