namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Maintenance;
    using Cyara.Domain.Types.Plan;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Foundation.Core.Utils;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Web.Contracts;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Web.Common.Constants;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Resources;
    using MediatR;

    public class ConfigurationViewModel : BaseViewModel, IValidatableObject, ITabbedView
    {
        public ConfigurationViewModel()
        {
            SelectedTab = "platform-wide";
            DashboardEnabled = DependencyResolver.Current.GetService<IDashboardRemote>() != null;
        }

        public IEnumerable<SettingViewData> Settings { get; set; }

        public IntHubConfigurationViewModel IntHubConfiguration { get; set; }

        public string SelectedTab { get; set; }

        public bool DashboardEnabled { get; set; }

        public bool VoiceSchedulerInMaintenanceMode { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Settings.Where(x => IsRequired(x.SettingKey)).Any(x => string.IsNullOrWhiteSpace(x.SettingValue)))
            {
                yield return new ValidationResult(
                    ValidationMessages.Required,
                    Settings.Where(x => IsRequired(x.SettingKey) && string.IsNullOrWhiteSpace(x.SettingValue)).Select(x => x.SettingKey).ToArray());
            }
            else
            {
                IList<AsrPoolEntity> asrPools = AsyncHelpers.RunSync(() => DependencyResolver.Current.GetService<IMediator>().Send(new AsrPoolsQuery { ActiveOnly = false }));
                var valid = string.Join(", ", asrPools.Where(x => x.IsActive).Select(x => x.AsrPool));

                var result = ValidateAsrPool(asrPools, valid, ConfigurationKey.AsrPool.Key);
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateAsrPool(asrPools, valid, ConfigurationKey.AsrPulsePool.Key);
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateBasedOnEnum<StorageBackend>(
                    ConfigurationKey.PlatformDefaultStorageBackend,
                    ValidationMessages.PlatformDefaultStorageBackend_NotValid,
                    true,
                    StorageBackend.Transient,
                    StorageBackend.None);

                if (result != null)
                {
                    yield return result;
                }

                result = ValidatePlatformDefaultPreConnectRetryAttempts();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidatePlatformDefaultTestCaseValidationLimit();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateCallRecordingAvailabilityDays();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateResultTranscriptionPlanTypes();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateEnhancedConfidenceAlgorithmPlanTypes();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateReliableConfidenceThresholdForLenientEnhancedConfidenceAlgorithm();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidatePlatformTestCaseGenerationLimit();
                if (result != null)
                {
                    yield return result;
                }

                result = ValidateBasedOnEnum<MaintenanceMode>(
                    ConfigurationKey.PlatformDefaultNewCallEngineMaintenanceMode,
                    ValidationMessages.PlatformDefaultNewCallEngineMaintenanceMode_NotValid,
                    true);

                result = ValidateCyaraAlmUrl();

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        private static bool IsRequired(string setting)
        {
            string[] notRequired =
                {
                    ConfigurationKey.GoogleAnalyticsTrackingCode.Key,
                    ConfigurationKey.GainsightPxAnalyticsTrackingCode.Key,
                    ConfigurationKey.GainsightPxPopCode.Key,
                    ConfigurationKey.GainsightPxRegionName.Key,
                    ConfigurationKey.FullStoryAnalyticsTrackingCode.Key,
                    ConfigurationKey.IntegrationHubUrl.Key,
                    ConfigurationKey.SupportUrl.Key,
                    ConfigurationKey.DeveloperCentralUrl.Key,
                    ConfigurationKey.AsrPool.Key,
                    ConfigurationKey.AsrPulsePool.Key, // These are required but they're validated separately based on the available pools in ValidateAsrPool()
                    ConfigurationKey.PlatformDefaultStorageBackend.Key, // This is also required, but validated based off available enum values in ValidatePlatformDefaultStorageBackend()
                    ConfigurationKey.PlatformDefaultNewCallEngineMaintenanceMode.Key, // This is also required, but validated based off available enum values in ValidatePlatformDefaultNewCallEngineStatus
                    ConfigurationKey.RolloutEnvironmentKey.Key, // Rollout.IO environment key used for feature switch
                    ConfigurationKey.CxInsightsUrl.Key,
                    ConfigurationKey.CxInsightsSecret.Key,
                    ConfigurationKey.MobileAppNotificationAlias.Key,
                    ConfigurationKey.ResultTranscriptionPlanTypes.Key,
                    ConfigurationKey.EnhancedConfidenceAlgorithm.Key,
                    ConfigurationKey.EnhancedConfidenceAlgorithmPlanTypes.Key,
                    ConfigurationKey.ReliableConfidenceThresholdForLenientEnhancedConfidenceAlgorithm.Key,
                    ConfigurationKey.CyaraPhoneUrl.Key,
                    ConfigurationKey.CyaraTfnSweepUrl.Key,
                    ConfigurationKey.AgentWebAppUrl.Key,
                    ConfigurationKey.AgentEndpointRealtimeUrl.Key,
                    ConfigurationKey.CyaraAlmUrl.Key,
                    ConfigurationKey.LiveVQ.Key,
                    ConfigurationKey.LoadAudioInspector.Key,
                    ConfigurationKey.LoadAudioStepMarkersFromDate.Key
                };

            var settingIsRequired = notRequired.All(x => !x.Equals(setting, StringComparison.OrdinalIgnoreCase));
            return settingIsRequired;
        }

        private ValidationResult ValidateAsrPool(IList<AsrPoolEntity> asrPools, string valid, string key)
        {
            SettingViewData asrPool = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (!string.IsNullOrEmpty(asrPool?.SettingValue))
            {
                var pool = asrPools.FirstOrDefault(a => a.AsrPool == asrPool.SettingValue);
                if (pool == null)
                {
                    return new ValidationResult(ValidationMessages.AsrPool_NotValid.FormatWith(valid), new[] { key });
                }

                if (!pool.IsActive)
                {
                    return new ValidationResult(ValidationMessages.AsrPool_NotActive.FormatWith(valid), new[] { key });
                }
            }

            return null;
        }

        private ValidationResult ValidateBasedOnEnum<TEnum>(ConfigurationKey configurationKey, string validationMessageFormatter, bool required, params TEnum[] invalidValues) where TEnum : struct
        {
            bool isValid = true;
            SettingViewData setting = Settings.FirstOrDefault(x => x.SettingKey == configurationKey.Key);

            if (string.IsNullOrWhiteSpace(setting?.SettingValue) && required)
            {
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(setting?.SettingValue))
            {
                if (!Enum.TryParse(setting.SettingValue, out TEnum value))
                {
                    isValid = false;
                }
                else if (invalidValues.Contains(value))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                string valid = string.Join(
                    ", ",
                    Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Where(e => !invalidValues.Contains(e)).Select(x => x.ToString()));

                return new ValidationResult(validationMessageFormatter.FormatWith(valid), new[] { configurationKey.Key });
            }

            return null;
        }

        private ValidationResult ValidatePlatformDefaultPreConnectRetryAttempts()
        {
            var key = ConfigurationKey.PlatformDefaultPreConnectRetryAttempts.Key;
            SettingViewData preConnectRetries = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (string.IsNullOrEmpty(preConnectRetries?.SettingValue) || !int.TryParse(preConnectRetries.SettingValue, out int attempts)
                                                                      || attempts < ConfigurationLimits.RetryAttemptsMin
                                                                      || attempts > ConfigurationLimits.RetryAttemptsMax)
            {
                return new ValidationResult(
                    ValidationMessages.PlatformDefaultPreConnectRetryAttempts_Range.FormatWith(
                        ConfigurationLimits.RetryAttemptsMin,
                        ConfigurationLimits.RetryAttemptsMax),
                    new[] { key });
            }

            return null;
        }

        private ValidationResult ValidatePlatformDefaultTestCaseValidationLimit()
        {
            var key = ConfigurationKey.PlatformDefaultTestCaseValidationLimit.Key;
            SettingViewData validationLimit = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (string.IsNullOrEmpty(validationLimit?.SettingValue) || !int.TryParse(validationLimit.SettingValue, out int limit) || limit < 1)
            {
                return new ValidationResult(ValidationMessages.PlatformDefaultTestCaseValidationLimit_Invalid, new[] { key });
            }

            return null;
        }

        private ValidationResult ValidateCallRecordingAvailabilityDays()
        {
            var key = ConfigurationKey.CallRecordingAvailabilityDays.Key;
            SettingViewData recordingAvailability = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (string.IsNullOrEmpty(recordingAvailability?.SettingValue) || !int.TryParse(recordingAvailability.SettingValue, out int days) || days < 0)
            {
                return new ValidationResult(ValidationMessages.CallRecordingAvailabilityDays_Invalid, new[] { key });
            }

            return null;
        }

        private ValidationResult ValidateReliableConfidenceThresholdForLenientEnhancedConfidenceAlgorithm()
        {
            var key = ConfigurationKey.ReliableConfidenceThresholdForLenientEnhancedConfidenceAlgorithm.Key;
            SettingViewData confidenceThreshold = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (!string.IsNullOrEmpty(confidenceThreshold?.SettingValue) && (!float.TryParse(confidenceThreshold.SettingValue, out float threshold) || threshold < 0f))
            {
                return new ValidationResult(ValidationMessages.ReliableConfidenceThresholdForLenientEnhancedConfidenceAlgorithm_Invalid, new[] { key });
            }

            return null;
        }

        private ValidationResult ValidateResultTranscriptionPlanTypes()
        {
            return ValidatePlanTypes(ConfigurationKey.ResultTranscriptionPlanTypes, ValidationMessages.ResultTranscriptionPlanTypes_Invalid);
        }

        private ValidationResult ValidateEnhancedConfidenceAlgorithmPlanTypes()
        {
            return ValidatePlanTypes(ConfigurationKey.EnhancedConfidenceAlgorithmPlanTypes, ValidationMessages.EnhancedConfidenceAlgorithmPlanTypes_Invalid);
        }

        private ValidationResult ValidateCyaraAlmUrl()
        {
            SettingViewData almUrl = Settings.FirstOrDefault(x => x.SettingKey == ConfigurationKey.CyaraAlmUrl.Key);

            return almUrl == null || string.IsNullOrEmpty(almUrl.SettingValue) || UrlParamExtensions.ValidateUrlSyntax(almUrl.SettingValue)
                   ? null
                   : new ValidationResult(ValidationMessages.AlmUrl_Invalid, new[] { almUrl.SettingKey });
        }

        private ValidationResult ValidatePlanTypes(ConfigurationKey configurationKey, string validationErrorMessage)
        {
            PlanType[] acceptablePlanTypes =
            {
                PlanType.Pulse, PlanType.Replay, PlanType.Velocity, PlanType.Cruncher, PlanType.CruncherLite, PlanType.Outbound, PlanType.PulseOutbound
            };

            var key = configurationKey.Key;

            ValidationResult ValidationResultFactory()
            {
                var msg = string.Format(
                    validationErrorMessage,
                    string.Join(", ", acceptablePlanTypes.Select(p => p.ToString())));

                return new ValidationResult(msg, new[] { key });
            }

            try
            {
                SettingViewData planTypes = Settings.FirstOrDefault(x => x.SettingKey == key);

                if (planTypes != null && !string.IsNullOrWhiteSpace(planTypes.SettingValue))
                {
                    var values = StringUtils.ParseEnumCsv<PlanType>(planTypes.SettingValue);
                    if (values.All(v => acceptablePlanTypes.Contains(v)))
                    {
                        return null;
                    }
                    else
                    {
                        return ValidationResultFactory();
                    }
                }
            }
            catch (Exception)
            {
                return ValidationResultFactory();
            }

            return null;
        }

        private ValidationResult ValidatePlatformTestCaseGenerationLimit()
        {
            var key = ConfigurationKey.TestCaseGenerationLimit.Key;
            SettingViewData generationLimit = Settings.FirstOrDefault(x => x.SettingKey == key);
            if (string.IsNullOrEmpty(generationLimit?.SettingValue) || !int.TryParse(generationLimit.SettingValue, out int limit) || limit < -1)
            {
                return new ValidationResult(ValidationMessages.PlatformTestCaseGenerationLimit_Invalid, new[] { key });
            }

            return null;
        }
    }
}
