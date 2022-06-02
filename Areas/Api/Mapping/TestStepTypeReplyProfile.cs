namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestStepTypeReplyProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestStepTypeReply, Api.Models.V2_5.TestStepTypeReply>();

            Mapper.CreateMap<Models.V2_5.TestStepTypeReply, Api.Models.V2_2.TestStepTypeReply>();
        }
    }
}