namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Xml.Serialization;

    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Resources;

    using Newtonsoft.Json;

    public partial class CampaignRun
    {
        /// <summary>
        /// Using field "[field name]Specified" we control visibility of related fields "[field name]", 
        /// <see cref="http://stackoverflow.com/questions/5818513/xml-serialization-hide-null-values"/>
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public bool EndDateSpecified { get; set; }

        public static CampaignRun From(Shared.Types.Campaign.CampaignRun run, IDataHelper dataHelper, string liveStatus = null)
        {
            if (run != null)
            {
                return new CampaignRun
                {
                    RunId = run.RunId,
                    StartDate = dataHelper.Output(run.StartDate),
                    EndDate = dataHelper.Output(run.EndDate),
                    Status = StatusFrom(run.Result, liveStatus)
                };
            }

            return null;
        }

        public static CampaignRunStatus StatusFrom(Cyara.Domain.Types.TestResult.ResultType result, string liveStatus = null)
        {
            if (liveStatus != null)
            {
                switch (liveStatus.ToLowerInvariant())
                {
                    case "running":
                        return CampaignRunStatus.Running;
                }
            }

            switch (result)
            {
                case Cyara.Domain.Types.TestResult.ResultType.Aborted:
                    return CampaignRunStatus.Aborted;
                case Cyara.Domain.Types.TestResult.ResultType.Failed:
                    return CampaignRunStatus.Failed;
                case Cyara.Domain.Types.TestResult.ResultType.InternalError:
                    return CampaignRunStatus.InternalError;
                case Cyara.Domain.Types.TestResult.ResultType.Satisfactory:
                    return CampaignRunStatus.Satisfactory;
                case Cyara.Domain.Types.TestResult.ResultType.Success:
                    return CampaignRunStatus.Success;
            }

            throw new Exception("Error handling CampaignRunStatus - unknown ResultType");
        }

        public static ApiResponse<CampaignRun> Load(ICampaignService campaignService, IAccountService accountService, IDataHelper dataHelper, User user, int account, string scope, int id)
        {
            // Get the campaign
            var runResponse = campaignService.CampaignRunGet(AccountRequest.Construct(id, user, account));
            runResponse.ExceptionIfError();

            if (runResponse.Value != null)
            {
                var planResponse = accountService.PlanGet(AccountRequest.Construct(runResponse.Value.Plan.PlanId, user, account));
                planResponse.ExceptionIfError();

                if (planResponse.Value != null && planResponse.Value.MediaType == ApiModel.TargetForScope(scope))
                {
                    return ApiResponse.Succeeds(From(runResponse.Value, dataHelper, runResponse.Value.Campaign.Status.ToString()));
                }
            }

            return ApiResponse.NotFoundId<CampaignRun>(ApiMessages.Entity_CampaignRun, id);
        }
    }
}