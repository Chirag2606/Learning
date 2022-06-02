namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CCMCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CCMCampaign, Api.Models.V2_4.CCMCampaign>()
                .ForMember(m => m.Pause, o => o.Ignore());
        }
    }
}