namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Responses;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Configuration;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Voice.Scheduler.Client.Contracts;
    using Cyara.Web.Messaging.Types.Command.VoiceSchedulerFeatures;
    using Cyara.Web.Messaging.Types.Data;
    using Cyara.Web.Messaging.Types.Query.VoiceSchedulerFeatures;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Extensions;

    using CyaraWebApi;

    using MediatR;

    using Newtonsoft.Json;

    using Component = Cyara.Foundation.Core.Settings.Component;
    using Severity = Cyara.Shared.Web.Models.Severity;

    public static class MaintenanceViewModelExtensions
    {
        public static async Task<MaintenanceViewModel> PrimeAsync(
            this MaintenanceViewModel value,
            IConfigurationService configurationService,
            CyaraWebApi.ComponentsClient componentsClient,
            IMediator mediator,
            bool applyDefaults)
        {
            if (applyDefaults)
            {
                bool portalInMaintenanceMode;
                value.MaintenanceModeForPortal = bool.TryParse(
                                                     configurationService.Get(ConfigurationKey.MaintenanceModePortal.Key),
                                                     out portalInMaintenanceMode) && portalInMaintenanceMode;

                var result = await componentsClient.GetComponentDataAsync(CyaraWebApi.Component.VoiceScheduler).ConfigureAwait(true);
                if (result == null)
                {
                    value.VoiceScheduler = new ComponentMaintenanceViewModel
                                               {
                                                   Component = Component.VoiceScheduler,
                                                   LimitedAvailabilityAccounts = string.Empty
                                               };
                }
                else
                {
                    value.VoiceScheduler = new ComponentMaintenanceViewModel
                                               {
                                                   Component = Component.VoiceScheduler,
                                                   LimitedAvailabilityAccounts = string.Join(",", result.LimitedAvailabilityAccounts),
                                               };
                }

                Action<IBaseResponse> featuresRetrievalFail = resp =>
                                                                  {
                                                                      value.VoiceSchedulerInstances = new Dictionary<string, InstanceViewData>();
                                                                      value.VoiceSchedulerFeatures = new Dictionary<string, string>();
                                                                      value.Message = new MessageViewData
                                                                                          {
                                                                                              Severity = Severity.PageFatal,
                                                                                              Text = new HtmlString(resp.ErrorMessage())
                                                                                          };
                                                                  };

                var instances = await mediator.Send(new VoiceSchedulerInstancesQuery()).ConfigureAwait(true);
                instances.ExceptionIfError();

                if (!instances.IsSuccess)
                {
                    featuresRetrievalFail(instances);
                    return value;
                }

                value.VoiceSchedulerFeatures = instances.Value.VoiceSchedulerFeatures;
                value.VoiceSchedulerInstances = instances.Value.VoiceSchedulerInstances.ToDictionary(
                    x => x.Key,
                    y => new InstanceViewData { InstanceName = y.Key, MaintenanceMode = y.Value.MaintenanceMode, LastUpdated = y.Value.LastUpdated.FormatToUtcDateTimeLong() });

                value.CallEngineChangesJson = string.Empty;
            }

            var allCallEngines = await componentsClient.GetInstancesListAsync(
                                         CyaraWebApi.Component.CallEngine,
                                         int.MaxValue,
                                         nameof(CyaraWebApi.InstanceDataModel.InstanceName),
                                         1,
                                         true)
                                     .ConfigureAwait(true);

            value.CallEngineInstances = allCallEngines.Results.ToDictionary(
                k => k.InstanceName,
                v => new InstanceViewData
                         {
                             InstanceName = v.InstanceName,
                             LastUpdated = v.LastUpdated.FormatToUtcDateTimeLong(),
                             LastUpdatedTicks = v.LastUpdated.Ticks,
                             MaintenanceMode = v.MaintenanceMode.FromApi()
                         });

            if (!string.IsNullOrEmpty(value.CallEngineChangesJson))
            {
                value.CallEngineChanges = JsonConvert.DeserializeObject<MaintenanceViewModel.CallEngineChangesModel>(value.CallEngineChangesJson);
            }

            return value;
        }

        public static async Task<bool> UpdateAsync(
            this MaintenanceViewModel value,
            ILogger logger,
            IConfigurationService configurationService,
            CyaraWebApi.ComponentsClient componentsClient,
            IMediator mediator,
            IVoiceSchedulerClient voiceSchedulerClient,
            SessionFacade session)
        {
            try
            {
                ConfigurationSetting[] configs = { new ConfigurationSetting { Key = ConfigurationKey.MaintenanceModePortal.Key, Value = value.MaintenanceModeForPortal.ToString() } };

                var updateResponse = configurationService.UpdateSettings(
                    new GenericRequest<IEnumerable<ConfigurationSetting>>(configs.AsEnumerable()) { User = session.User });

                updateResponse.ExceptionIfError();

                if (!updateResponse.IsSuccess)
                {
                    value.Message = new MessageViewData { Severity = Severity.ValidationError, Text = new HtmlString(updateResponse.ErrorMessage()) };
                    return false;
                }

                ICollection<int> limitedAvailAccounts;
                if (!string.IsNullOrWhiteSpace(value.VoiceScheduler.LimitedAvailabilityAccounts))
                {
                    limitedAvailAccounts = value.VoiceScheduler.LimitedAvailabilityAccounts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()))
                        .ToArray();
                }
                else
                {
                    limitedAvailAccounts = new int[0];
                }

                await componentsClient.UpdateComponentAsync(
                        CyaraWebApi.Component.VoiceScheduler,
                        new MaintenanceDataUpdateModel { LimitedAvailabilityAccounts = limitedAvailAccounts })
                    .ConfigureAwait(true);

                var roleSave = VoiceSchedulerFeaturesSaveCommand.Construct(session);
                roleSave.VoiceSchedulerFeatures = value.VoiceSchedulerFeatures;
                roleSave.VoiceSchedulerInstances =
                    value.VoiceSchedulerInstances?.ToDictionary(x => x.Key, y => new VoiceSchedulerInstanceData { MaintenanceMode = y.Value.MaintenanceMode })
                    ?? new Dictionary<string, VoiceSchedulerInstanceData>();
                var roleSaveResponse = await mediator.Send(roleSave).ConfigureAwait(true);
                roleSaveResponse.ExceptionIfError();

                if (!roleSaveResponse.IsSuccess)
                {
                    value.Message = new MessageViewData { Severity = Severity.ValidationError, Text = new HtmlString(roleSaveResponse.ErrorMessage()) };
                    return false;
                }

                bool notifyVoiceSchedulerRegardingCEChanges = false;
                if (value.CallEngineChanges != null)
                {
                    foreach (var ceChange in value.CallEngineChanges.Changed)
                    {
                        await componentsClient.UpdateInstanceAsync(
                            CyaraWebApi.Component.CallEngine,
                            ceChange.Key,
                            new InstanceUpdateModel { MaintenanceMode = ceChange.Value.ToApi() }).ConfigureAwait(true);

                        notifyVoiceSchedulerRegardingCEChanges = true;
                    }

                    foreach (var ceDelete in value.CallEngineChanges.Deleted)
                    {
                        await componentsClient.DeleteInstanceAsync(CyaraWebApi.Component.CallEngine, ceDelete).ConfigureAwait(true);

                        notifyVoiceSchedulerRegardingCEChanges = true;
                    }
                }

                if (notifyVoiceSchedulerRegardingCEChanges)
                {
                    try
                    {
                        await voiceSchedulerClient.NotifyCallEngineStateChangedAsync().ConfigureAwait(true);
                    }
                    catch (Exception exc)
                    {
                        logger.LogError(typeof(MaintenanceViewModelExtensions), $"Successfully updated maintenance mode data, but failed to notify voice scheduler. {exc.ToTypedFlattenedInnerExceptionMessage()}");
                        value.PageWarning = "PlatformMaintenanceUnableToNotify";
                    }
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Exception(typeof(MaintenanceViewModelExtensions), $"Unexpected exception in {nameof(UpdateAsync)}", exc);

                value.Message = new MessageViewData().Prime("PlatformMaintenanceFailed", Severity.ValidationError);

                return false;
            }
        }
    }
}
