﻿namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using AutoMapper;

    public class CruncherCampaignProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2_2.CruncherCampaign, Api.Models.V2_4.CruncherCampaign>();
        }
    }
}