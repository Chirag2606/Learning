namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class AgentStatus
    {
        public static AgentStatus From(Cyara.Shared.Web.Types.Scheduler.CCMAgentStatus agent)
        {
            var firstName = string.Empty;
            var lastName = string.Empty;

            var cleanName = (agent.Name ?? string.Empty).Trim();

            var splitPoint = cleanName.IndexOfAny(new[] { ' ', ',' });
            if (splitPoint != -1)
            {
                firstName = cleanName.Substring(0, splitPoint).Replace(",", " ").Trim();
                lastName = cleanName.Substring(splitPoint).Replace(",", " ").Trim();
            }

            return new AgentStatus
            {
                AgentId = agent.AgentId,
                Name = firstName,
                CallsReceived = agent.InteractionsReceived,
                Status = agent.Status ?? string.Empty,
                Behaviour = agent.Behaviour ?? string.Empty,
                Duration = TimeSpan.From(agent.Duration),
            };
        }

        public static AgentStatus[] ArrayFrom(List<Cyara.Shared.Web.Types.Scheduler.CCMAgentStatus> agents)
        {
            if (agents == null || agents.Count <= 0)
            {
                return null;
            }

            return agents.Select(x => AgentStatus.From(x)).ToArray();
        }
    }
}