namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Maintenance;
    using Cyara.Foundation.Core.Messages;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Web.Common;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Extensions;

    using Component = CyaraWebApi.Component;

    public static class SchedulerStatusViewModelExtensions
    {
        private static readonly Type Me = typeof(SchedulerStatusViewModelExtensions);

        public static async Task<SchedulerStatusViewModel> PrimeAsync(
            this SchedulerStatusViewModel model,
            ISchedulerService schedulerService,
            CyaraWebApi.ComponentsClient componentsClient,
            IConfigurationService configurationService,
            ILogger logger,
            SessionFacade session,
            UrlHelper urlHelper,
            bool applyDefaults)
        {
            if (applyDefaults)
            {
                model.Status = new VoiceSchedulerStatusViewData[0];
                model.CallEngines = new CallEngineViewData[0];
                model.QueuedCampaigns = new QueuedCampaignViewData[0];
                model.RunningCampaigns = new RunningCampaignViewData[0];
                model.LastRefreshed = DateTime.UtcNow.FormatToUserLocalDateTime();
            }

            try
            {
                // has to return to the same thread, otherwise automapper fails shamefully as it doesn't have http context
                var status = await schedulerService.SchedulerStatus(new BaseRequest { User = session.User }).ConfigureAwait(true);
                status.ExceptionIfError();

                Mapper.Map<SchedulerStatus, SchedulerStatusViewModel>(status.Value, model);

                if (!model.Status.Any())
                {
                    // no status info items collected, that's an error scenario
                    model.Message = new MessageViewData
                                        {
                                            Text = new HtmlString(
                                                new SystemMessage(ErrorCodes.CommunicationError, SubSystem.None).GetDisplayMessage()),
                                            Severity = Severity.ValidationError
                                        };
                }

                Lazy<MaintenanceMode> platformDefault = new Lazy<MaintenanceMode>(
                    () =>
                        {
                            var value = configurationService.Get(ConfigurationKey.PlatformDefaultNewCallEngineMaintenanceMode.Key);
                            if (Enum.TryParse(value, false, out MaintenanceMode result))
                            {
                                return result;
                            }

                            return MaintenanceMode.GracefulShutdown;
                        });

                var callEnginesMaintMode = await componentsClient.GetInstancesListAsync(Component.CallEngine, int.MaxValue, nameof(CyaraWebApi.InstanceDataModel.InstanceName), 1, true).ConfigureAwait(false);

                foreach (var ce in model.CallEngines)
                {
                    var dbRecord = callEnginesMaintMode.Results.FirstOrDefault(x => x.InstanceName == ce.UniqueCallEngineId);
                    if (dbRecord != null)
                    {
                        ce.MaintenanceMode = dbRecord.MaintenanceMode.FromApi().ToDisplay();
                    }
                    else
                    {
                        // we have a connected CE that does not exist in table yet
                        // this is a temporary situation which will resolve itself via further refreshes
                        // at this stage, just use default from tblConfig for display purposes
                        ce.MaintenanceMode = platformDefault.Value.ToDisplay();
                    }
                }

                string campaignUriTemplate = urlHelper.RouteUrl(
                    "CampaignWithMedia",
                    new RouteValueDictionary
                        {
                            { "routeAccountId", "-replace-accountid-" },
                            { "id", "-replace-campaignid-" },
                            { "media", MediaType.Voice }
                        });

                if (campaignUriTemplate == null)
                {
                    throw new Exception("Failed to generate a template route to campaign edit page");
                }

                foreach (var queuedJob in model.QueuedCampaigns)
                {
                    if (queuedJob.CampaignId > 0)
                    {
                        queuedJob.LinkToCampaign = campaignUriTemplate.Replace("-replace-campaignid-", $"{queuedJob.CampaignId}")
                            .Replace("-replace-accountid-", $"{queuedJob.AccountId}");
                    }
                }

                foreach (var runningJob in model.RunningCampaigns)
                {
                    if (runningJob.CampaignId > 0)
                    {
                        runningJob.LinkToCampaign = campaignUriTemplate.Replace("-replace-campaignid-", $"{runningJob.CampaignId}")
                            .Replace("-replace-accountid-", $"{runningJob.AccountId}");
                    }
                }

                return model;
            }
            catch (Exception exc)
            {
                logger.Exception(Me, "Unexpected exception getting scheduler status", exc);

                model.Message = new MessageViewData
                                    {
                                        Text = new HtmlString(new SystemMessage(ErrorCodes.UnknownError, SubSystem.None).GetDisplayMessage()),
                                        Severity = Severity.ValidationError
                                    };
            }

            return model;
        }
    }
}
