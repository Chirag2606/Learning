namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class TestStepResultListProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.TestStepResultList, Api.Models.V2_5.TestStepResultList>()
                .AfterMap((m, p) =>
                              {
                                  if (m.AgentDataStepResult == null)
                                  {
                                      p.AgentDataStepResult = null;
                                  }
                              });

            Mapper.CreateMap<Models.V2_5.TestStepResultList, Api.Models.V2_2.TestStepResultList>()
                .ForMember(m => m.AgentDataStepResult, o => o.Ignore());
        }
    }
}