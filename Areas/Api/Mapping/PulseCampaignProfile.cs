namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class PulseCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.PulseCampaign, Api.Models.V2_4.PulseCampaign>()
                .ForMember(m => m.Pause, o => o.Ignore());
        }
    }
}