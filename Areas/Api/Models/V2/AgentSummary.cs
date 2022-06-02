namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Shared.Types.Agent;

    public partial class AgentSummary
    {
        public static AgentSummary From(Agent agent)
        {
            if (agent == null)
            {
                return null;
            }

            return new AgentSummary
            {
                AgentId = agent.AgentId,
                EmployeeId = agent.Name
            };
        }
    }
}