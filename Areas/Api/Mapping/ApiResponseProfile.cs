namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_4;

    public class ApiResponseProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<ApiResponse<Models.V2_2.CCMCampaign>, ApiResponse<CCMCampaign>>();

            Mapper.CreateMap<ApiResponse<Models.V2_2.Campaign>, ApiResponse<Campaign>>();

            Mapper.CreateMap<ApiResponse<Models.V2_2.CampaignList>, ApiResponse<CampaignList>>();
        }
    }
}