namespace Cyara.Web.Portal.Areas.Api.Mapping 
{
    using AutoMapper;

    public class OutboundCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.OutboundCampaign, Api.Models.V2_4.OutboundCampaign>();
        }
    }
}