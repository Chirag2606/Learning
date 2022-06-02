namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CCMCampaignScheduleProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CCMCampaignSchedule, Api.Models.V2_4.CCMCampaignSchedule>();
        }
    }
}