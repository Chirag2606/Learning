namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestStepRingTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestStepRingType, Api.Models.V2_5.TestStepRingType>();

            Mapper.CreateMap<Models.V2_5.TestStepRingType, Api.Models.V2_2.TestStepRingType>();
        }
    }
}