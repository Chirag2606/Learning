namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestCaseProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestCase, Models.V2_5.TestCase>()
                .ForMember(m => m.Type, o => o.UseValue(Models.V2_5.Channel.Voice))
                .ForMember(m => m.EmailTo, o => o.Ignore())
                .ForMember(m => m.Mobile, o => o.Ignore())
                .ForMember(m => m.Url, o => o.Ignore());
        }
    }
}