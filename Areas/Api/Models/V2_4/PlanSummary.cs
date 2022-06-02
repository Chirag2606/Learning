namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using System.Linq;

    using Cyara.Shared.Types.Campaign;

    public partial class PlanSummary
    {
        private static readonly PlanMap[] PlanMapList =
        {
            new PlanMap { ApiPlan = PlanType.VirtualAgent, ModelPlan = Cyara.Domain.Types.Plan.PlanType.VirtualAgent },
            new PlanMap { ApiPlan = PlanType.Cruncher, ModelPlan = Cyara.Domain.Types.Plan.PlanType.Cruncher },
            new PlanMap { ApiPlan = PlanType.CruncherLite, ModelPlan = Cyara.Domain.Types.Plan.PlanType.CruncherLite },
            new PlanMap { ApiPlan = PlanType.Outbound, ModelPlan = Cyara.Domain.Types.Plan.PlanType.Outbound },
            new PlanMap { ApiPlan = PlanType.Pulse, ModelPlan = Cyara.Domain.Types.Plan.PlanType.Pulse },
            new PlanMap { ApiPlan = PlanType.PulseOutbound, ModelPlan = Cyara.Domain.Types.Plan.PlanType.PulseOutbound },
            new PlanMap { ApiPlan = PlanType.Replay, ModelPlan = Cyara.Domain.Types.Plan.PlanType.Replay },
            new PlanMap { ApiPlan = PlanType.Velocity, ModelPlan = Cyara.Domain.Types.Plan.PlanType.Velocity }
        };

        public static PlanSummary From(ICampaign campaign)
        {
            if (campaign?.Plan == null || campaign.Plan.PlanType == 0)
            {
                return null;
            }

            return new PlanSummary
                   {
                       PlanId = campaign.Plan.PlanId,
                       PlanType = PlanMapList.First(x => x.ModelPlan == campaign.Plan.PlanType).ApiPlan
                   };
        }

        private struct PlanMap
        {
            public PlanType ApiPlan;

            public Cyara.Domain.Types.Plan.PlanType ModelPlan;
        }
    }
}