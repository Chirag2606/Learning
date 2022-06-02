namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestStepTypeExpectProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestStepTypeExpect, Api.Models.V2_5.TestStepTypeExpect>();

            Mapper.CreateMap<Models.V2_5.TestStepTypeExpect, Api.Models.V2_2.TestStepTypeExpect>();
        }
    }
}