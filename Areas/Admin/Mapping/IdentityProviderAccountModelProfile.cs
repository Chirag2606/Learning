namespace Cyara.Web.Portal.Areas.Admin.Mapping
{
    using AutoMapper;
    
    using Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider;

    public class IdentityProviderAccountModelProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<CyaraWebApi.IdentityProviderAccountModel, IdentityProviderAccountViewData>();
        }
    }
}