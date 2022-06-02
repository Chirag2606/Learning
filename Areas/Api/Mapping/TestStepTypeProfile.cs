namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestStepTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestStepType, Api.Models.V2_5.TestStepType>();

            Mapper.CreateMap<Models.V2_5.TestStepType, Api.Models.V2_2.TestStepType>();
        }
    }
}