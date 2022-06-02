namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.Campaign, Api.Models.V2_4.Campaign>().ForMember(m => m.Velocity, o => o.Ignore());
        }
    }
}