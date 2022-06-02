namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class AgentSummaryProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.AgentSummary, Api.Models.V2_4.AgentSummary>();
        }
    }
}