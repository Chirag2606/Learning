namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class SchedulePeriodProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.SchedulePeriod, Api.Models.V2_4.SchedulePeriod>();
        }
    }
}