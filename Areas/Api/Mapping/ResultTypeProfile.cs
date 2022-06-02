namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class ResultTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.ResultType, Api.Models.V2_5.ResultType>();
        }
    }
}