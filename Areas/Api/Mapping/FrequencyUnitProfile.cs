namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class FrequencyUnitProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.FrequencyUnit, Api.Models.V2_4.FrequencyUnit>();
        }
    }
}