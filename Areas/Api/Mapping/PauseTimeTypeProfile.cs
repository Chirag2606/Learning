namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class PauseTimeTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.PauseTimeType, Api.Models.V2_5.PauseTimeType>();

            Mapper.CreateMap<Models.V2_5.PauseTimeType, Api.Models.V2_2.PauseTimeType>();
        }
    }
}