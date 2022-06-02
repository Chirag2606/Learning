namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CampaignListProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CampaignList, Api.Models.V2_4.CampaignList>();
        }
    }
}