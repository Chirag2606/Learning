namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class ThresholdTimeTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.ThresholdTimeType, Api.Models.V2_5.ThresholdTimeType>();

            Mapper.CreateMap<Models.V2_5.ThresholdTimeType, Api.Models.V2_2.ThresholdTimeType>();
        }
    }
}