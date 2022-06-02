namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    using Cyara.Web.Messaging.Types.User;

    public class ApiKeyCredentialsProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<ApiKeyCredentials, Api.Models.V2_5.ApiCredentials>();
        }
    }
}