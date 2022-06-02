namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestCaseAlertProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestCaseAlert, Api.Models.V2_5.TestCaseAlert>();
        }
    }
}