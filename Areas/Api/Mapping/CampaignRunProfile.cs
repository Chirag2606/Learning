namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CampaignRunProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2.CampaignRun, Api.Models.V2_5.CampaignRun>();
        }
    }
}