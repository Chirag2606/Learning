namespace Cyara.Web.Portal.Areas.Admin.Mapping
{
    using System;

    using AutoMapper;

    using Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider;

    public class IdentityProviderViewModelProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<IdentityProviderViewModel, CyaraWebApi.IdentityProviderModel>()
                .ForMember(m => m.ProviderId, o => o.MapFrom(s => s.ProviderId ?? string.Empty))
                .ForMember(m => m.Name, o => o.MapFrom(s => s.Name ?? string.Empty))
                .ForMember(
                    m => m.Type,
                    o => o.MapFrom(s => (CyaraWebApi.LoginProvider)Enum.Parse(typeof(CyaraWebApi.LoginProvider), s.LoginProvider.ToString())))
                .ForMember(m => m.Settings, o => o.MapFrom(s => Mapper.Map<CyaraWebApi.SamlProviderSettingsModel>(s)));

            Mapper.CreateMap<IdentityProviderViewModel, CyaraWebApi.SamlProviderSettingsModel>();
        }
    }
}