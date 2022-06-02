namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CampaignInfoProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CampaignInfo, Api.Models.V2_5.CampaignInfo>();
        }
    }
}