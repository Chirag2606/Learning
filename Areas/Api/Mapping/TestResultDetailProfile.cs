namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestResultDetailProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestResultDetail, Api.Models.V2_5.TestResultDetail>();

            Mapper.CreateMap<Models.V2_5.TestResultDetail, Api.Models.V2_2.TestResultDetail>();
        }
    }
}