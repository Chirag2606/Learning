namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TimeSpanProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TimeSpan, Api.Models.V2_5.TimeSpan>();

            Mapper.CreateMap<Models.V2_5.TimeSpan, Api.Models.V2_2.TimeSpan>();
        }
    }
}