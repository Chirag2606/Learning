namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    using Cyara.Web.Portal.Areas.Api.Models.V2_5;

    public class TestCaseResult2WithCampaignInfoProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestCaseResult2WithCampaignInfo, Api.Models.V2_5.TestCaseResult2WithCampaignInfo>()
                .ForMember(m => m.Type, o => o.UseValue(Channel.Voice))
                .ForMember(m => m.EmailTo, o => o.Ignore())
                .ForMember(m => m.Mobile, o => o.Ignore())
                .ForMember(m => m.Url, o => o.Ignore());
        }
    }
}