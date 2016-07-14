using AutoMapper;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
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
                .ForMember(co => co.OverriddenPrice, i => i.MapFrom(p => Math.Truncate(p.OverridePrice * 10)))
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
                .ForMember(co => co.CompetitorPriceOffset, i => i.MapFrom(p => p.CompetitorPriceOffset));
        }
    }
}