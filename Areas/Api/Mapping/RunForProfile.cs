namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class RunForProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.RunFor, Api.Models.V2_4.RunFor>();
        }
    }
}