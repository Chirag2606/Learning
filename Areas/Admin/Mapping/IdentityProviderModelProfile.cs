namespace Cyara.Web.Portal.Areas.Admin.Mapping
{
    using AutoMapper;

    using Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider;
    using Cyara.Web.Portal.Core.Extensions;

    public class IdentityProviderModelProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<CyaraWebApi.IdentityProviderModel, IdentityProviderViewModel>()
                .ForMember(m => m.LoginProviderList, o => o.Ignore())
                .ForMember(m => m.AssertionConsumerService, o => o.Ignore())
                .ForMember(m => m.ServiceProviderEntity, o => o.Ignore())
                .ForMember(m => m.TrackingCodes, o => o.Ignore())
                .ForMember(m => m.SiteVersion, o => o.Ignore())
                .ForMember(m => m.CopyrightYear, o => o.Ignore())
                .ForMember(m => m.SelectedAccountId, o => o.Ignore())
                .ForMember(m => m.SelectedAccountName, o => o.Ignore())
                .ForMember(m => m.ServerDateTime, o => o.Ignore())
                .ForMember(m => m.ServerDateTimeZone, o => o.Ignore())
                .ForMember(m => m.Message, o => o.Ignore())
                .ForMember(m => m.User, o => o.Ignore())
                .ForMember(
                    m => m.LoginProvider,
                    o => o.MapFrom(s => s.Type.FromApi()))
                .ForMember(m => m.Certificate, o => o.MapFrom(s => s.Settings.Certificate))
                .ForMember(m => m.IdentityProviderUrl, o => o.MapFrom(s => s.Settings.IdentityProviderUrl))
                .ForMember(m => m.SingleSignOnUrl, o => o.MapFrom(s => s.Settings.SingleSignOnUrl))
                .ForMember(m => m.Metadata, o => o.MapFrom(s => s.Settings.Metadata))
                .ForMember(m => m.MetadataUrl, o => o.MapFrom(s => s.Settings.MetadataUrl));
        }
    }
}