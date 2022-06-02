namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Agent;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Web.Resources;

    public partial class AgentBehaviour
    {
        public static ApiResponse SetOn(AgentBehaviour[] tests, Shared.Types.Campaign.Campaign campaign, IAgentService agentService, ApiSessionFacade session, int accountId)
        {
            if (tests.Any(x => x.Agent == null || x.Agent.AgentId <= 0))
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Agent/AgentId"), ApiMessages.Content);
            }

            if (tests.Any(x => x.Behaviour == null || x.Behaviour.BehaviourId <= 0))
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Behaviour/BehaviourId"), ApiMessages.Content);
            }

            if (tests.Any(x => x.Server == null || x.Server.ServerId <= 0))
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Server/ServerId"), ApiMessages.Content);
            }

            // Ensure that the same agent hasn't been specified twice
            var dup = tests.GroupBy(x => x.Agent.AgentId).FirstOrDefault(x => x.Count() > 1);
            if (dup != null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_DuplicateValue.FormatWith("Agent/AgentId", dup.Key), ApiMessages.Content);
            }

            if (campaign.Agents == null)
            {
                campaign.Agents = new List<CampaignAgentBehaviour>();
            }

            var sitesResponse = agentService.SiteCrud.GetByAccount(new AccountRequest { AccountId = accountId, User = session.User });
            sitesResponse.ExceptionIfError();

            if (sitesResponse.Value == null || sitesResponse.Value.Count == 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.SitesNotConfigured, ApiMessages.Content);
            }

            foreach (var test in tests)
            {
                var cab = campaign.Agents.FirstOrDefault(x => x.AgentId == test.Agent.AgentId);
                if (cab != null)
                {
                    campaign.Agents.Remove(cab);
                }

                campaign.Agents.Add(new CampaignAgentBehaviour
                                    {
                                        AgentId = test.Agent.AgentId,
                                        BehaviourId = test.Behaviour.BehaviourId,
                                        ServerId = test.Server.ServerId,
                                        SiteId = sitesResponse.Value.First().SiteId
                                    });
            }

            return ApiResponse.Succeeds();
        }
    }
}
