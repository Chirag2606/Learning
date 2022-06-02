namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class RingStepResultProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.RingStepResult, Api.Models.V2_5.RingStepResult>();

            Mapper.CreateMap<Models.V2_5.RingStepResult, Api.Models.V2_2.RingStepResult>();
        }
    }
}