namespace Cyara.Web.Portal.Areas.Admin.Mapping
{
    using AutoMapper;

    using Cyara.Shared.Types.Identity;

    public class SamlProviderSettingsModelProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<CyaraWebApi.SamlProviderSettingsModel, SamlProviderSettings>();
        }
    }
}