namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestCaseProbabilityProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestCaseProbability, Api.Models.V2_4.TestCaseProbability>();
        }
    }
}