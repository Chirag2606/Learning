namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestResultRequestProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestResultRequest, Api.Models.V2_5.TestResultRequest>();
        }
    }
}