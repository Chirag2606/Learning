namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class ServerSummaryProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.ServerSummary, Api.Models.V2_4.ServerSummary>();
        }
    }
}