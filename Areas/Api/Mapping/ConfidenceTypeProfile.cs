namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class ConfidenceTypeProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.ConfidenceType, Api.Models.V2_5.ConfidenceType>();

            Mapper.CreateMap<Models.V2_5.ConfidenceType, Api.Models.V2_2.ConfidenceType>();
        }
    }
}