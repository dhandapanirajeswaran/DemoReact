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
            
        }
    }
}