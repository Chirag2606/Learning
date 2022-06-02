namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class DialResultDetailProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.DialResultDetail, Api.Models.V2_5.DialResultDetail>();
        }
    }
}