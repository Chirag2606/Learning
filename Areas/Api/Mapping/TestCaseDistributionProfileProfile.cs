namespace Cyara.Web.Portal.Areas.Api.Mapping
{
    using System;

    using AutoMapper;

    public class TestCaseDistributionProfileProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Models.V2.TestCaseDistributionProfile, Domain.Types.Campaign.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                        {
                            switch (profile)
                            {
                                case Models.V2.TestCaseDistributionProfile.UserDefinedProbability:
                                    return Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined;

                                case Models.V2.TestCaseDistributionProfile.RoundRobin:
                                    return Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin;

                                case Models.V2.TestCaseDistributionProfile.EqualProbability:
                                    return Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability;

                                case Models.V2.TestCaseDistributionProfile.SequentialConditional:
                                    return Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional;

                                default:
                                    throw new Exception($"Unknown value for mapping: {profile}");
                            }
                        });

            Mapper.CreateMap<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                        {
                            switch (profile)
                            {
                                case Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined:
                                    return Models.V2.TestCaseDistributionProfile.UserDefinedProbability;

                                case Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin:
                                case Domain.Types.Campaign.TestCaseDistributionProfile.NotApplicable:
                                    return Models.V2.TestCaseDistributionProfile.RoundRobin;

                                case Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability:
                                    return Models.V2.TestCaseDistributionProfile.EqualProbability;

                                case Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional:
                                    return Models.V2.TestCaseDistributionProfile.SequentialConditional;

                                // ReSharper disable once RedundantCaseLabel - need to explicitly stae that Unknown will trigger exception
                                case Domain.Types.Campaign.TestCaseDistributionProfile.Unknown:
                                default:
                                    throw new Exception($"Unknown value for mapping: {profile}");
                            }
                        });

            Mapper.CreateMap<Models.V2_2.TestCaseDistributionProfile, Domain.Types.Campaign.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                    {
                        switch (profile)
                        {
                            case Models.V2_2.TestCaseDistributionProfile.UserDefinedProbability:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined;

                            case Models.V2_2.TestCaseDistributionProfile.RoundRobin:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin;

                            case Models.V2_2.TestCaseDistributionProfile.EqualProbability:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability;

                            case Models.V2_2.TestCaseDistributionProfile.SequentialConditional:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional;

                            default:
                                throw new Exception($"Unknown value for mapping: {profile}");
                        }
                    });

            Mapper.CreateMap<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2_2.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                    {
                        switch (profile)
                        {
                            case Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined:
                                return Models.V2_2.TestCaseDistributionProfile.UserDefinedProbability;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin:
                            case Domain.Types.Campaign.TestCaseDistributionProfile.NotApplicable:
                                return Models.V2_2.TestCaseDistributionProfile.RoundRobin;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability:
                                return Models.V2_2.TestCaseDistributionProfile.EqualProbability;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional:
                                return Models.V2_2.TestCaseDistributionProfile.SequentialConditional;

                            // ReSharper disable once RedundantCaseLabel - need to explicitly stae that Unknown will trigger exception
                            case Domain.Types.Campaign.TestCaseDistributionProfile.Unknown:
                            default:
                                throw new Exception($"Unknown value for mapping: {profile}");
                        }
                    });

            Mapper.CreateMap<Models.V2_4.TestCaseDistributionProfile, Domain.Types.Campaign.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                    {
                        switch (profile)
                        {
                            case Models.V2_4.TestCaseDistributionProfile.UserDefinedProbability:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined;

                            case Models.V2_4.TestCaseDistributionProfile.RoundRobin:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin;

                            case Models.V2_4.TestCaseDistributionProfile.EqualProbability:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability;

                            case Models.V2_4.TestCaseDistributionProfile.SequentialConditional:
                                return Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional;

                            default:
                                throw new Exception($"Unknown value for mapping: {profile}");
                        }
                    });

            Mapper.CreateMap<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2_4.TestCaseDistributionProfile>()
                .ConvertUsing(
                    profile =>
                    {
                        switch (profile)
                        {
                            case Domain.Types.Campaign.TestCaseDistributionProfile.UserDefined:
                                return Models.V2_4.TestCaseDistributionProfile.UserDefinedProbability;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.RoundRobin:
                            case Domain.Types.Campaign.TestCaseDistributionProfile.NotApplicable:
                                return Models.V2_4.TestCaseDistributionProfile.RoundRobin;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.EqualProbability:
                                return Models.V2_4.TestCaseDistributionProfile.EqualProbability;

                            case Domain.Types.Campaign.TestCaseDistributionProfile.SequentialConditional:
                                return Models.V2_4.TestCaseDistributionProfile.SequentialConditional;

                            // ReSharper disable once RedundantCaseLabel - need to explicitly stae that Unknown will trigger exception
                            case Domain.Types.Campaign.TestCaseDistributionProfile.Unknown:
                            default:
                                throw new Exception($"Unknown value for mapping: {profile}");
                        }
                    });
        }
    }
}