namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class BehaviourSummaryProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.BehaviourSummary, Api.Models.V2_4.BehaviourSummary>();
        }
    }
}