namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class PulseOutboundCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.PulseOutboundCampaign, Api.Models.V2_4.PulseOutboundCampaign>()
                .ForMember(m => m.Pause, o => o.Ignore());
        }
    }
}