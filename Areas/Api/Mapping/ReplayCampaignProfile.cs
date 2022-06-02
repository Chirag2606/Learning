namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class ReplayCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.ReplayCampaign, Api.Models.V2_4.ReplayCampaign>();
        }
    }
}