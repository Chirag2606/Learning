namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CallStepResultProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CallStepResult, Api.Models.V2_5.CallStepResult>().ForMember(m => m.ServiceStepResult, o => o.Ignore());

            Mapper.CreateMap<Models.V2_2.AgentDataStepResult, Api.Models.V2_5.AgentDataStepResult>();

            Mapper.CreateMap<Models.V2_2.AgentKeyValuePair, Api.Models.V2_5.AgentKeyValuePair>();

            Mapper.CreateMap<Models.V2_2.TestStepAgentDataType, Api.Models.V2_5.TestStepAgentDataType>();

            Mapper.CreateMap<Models.V2_5.CallStepResult, Api.Models.V2_2.CallStepResult>();

            Mapper.CreateMap<Models.V2_5.AgentDataStepResult, Api.Models.V2_2.AgentDataStepResult>();

            Mapper.CreateMap<Models.V2_5.AgentKeyValuePair, Api.Models.V2_2.AgentKeyValuePair>();

            Mapper.CreateMap<Models.V2_5.TestStepAgentDataType, Api.Models.V2_2.TestStepAgentDataType>();
        }
    }
}