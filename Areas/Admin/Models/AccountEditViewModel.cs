namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Domain.Business.Collections;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Rules;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Collections;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Common.Constants;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Validation;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    public class AccountEditViewModel : BaseViewModel, IValidatableObject
    {
        private string _confidenceThreshold;

        private string _mosThreshold;

        [Required(ErrorMessage = " ")]
        [RegularExpression(@"^[^\\\\/]*$", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "AccountName_Invalid")]
        [StringLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "AccountName_WrongSize", MinimumLength = 1)]
        [Display(Name = "AccountName", ResourceType = typeof(Labels))]
        public string Name { get; set; }

        public bool Active { get; set; }

        [Display(Name = "AsrPool", ResourceType = typeof(Labels))]
        public string AsrPool { get; set; }

        [Display(Name = "AsrPulsePool", ResourceType = typeof(Labels))]
        public string AsrPulsePool { get; set; }

        [Range(ConfigurationLimits.RetryAttemptsMin, ConfigurationLimits.RetryAttemptsMax, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "PreConnectRetryAttempts_Invalid")]
        [Display(Name = "PreConnectRetryAttempts", ResourceType = typeof(Labels))]
        public int PreConnectRetryAttempts { get; set; }

        [Display(Name = "OverridePreConnectRetryAttemptsPlatformDefault", ResourceType = typeof(Labels))]
        public bool OverridePreConnectRetryAttemptsPlatformDefault { get; set; }

        [Range(0, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "TestCaseValidationLimit_Invalid")]
        [Display(Name = "TestCaseValidationLimit", ResourceType = typeof(Labels))]
        public int TestCaseValidationLimit { get; set; }

        public bool OverridePlatformDefaultTestCaseValidationLimit { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "StorageBackend", ResourceType = typeof(Labels))]
        public string StorageBackend { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Account_RecognitionLanguageDisabled")]
        [Display(Name = "RecognitionLanguage", ResourceType = typeof(Labels))]
        public string RecognitionLanguage { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Account_ReplyVoiceDisabled")]
        [Display(Name = "ReplyVoice", ResourceType = typeof(Labels))]
        public string ReplyVoice { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "Country", ResourceType = typeof(Labels))]
        public string Country { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "ActivationDate", ResourceType = typeof(Labels))]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime Activation { get; set; }

        [RequiredIf("NeverExpires", false, ErrorMessage = " ")]
        [Display(Name = "ExpiryDate", ResourceType = typeof(Labels))]
        [GreaterThan("Activation", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Expiry_BeforeActivation")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime? Expiry { get; set; }

        [Display(Name = "NeverExpires", ResourceType = typeof(Labels))]
        public bool NeverExpires { get; set; }

        [Range(0.01F, Domain.Types.Rules.Plans.Voice.MaxCaps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxCaps_Invalid")]
        [Display(Name = "MaxCaps", ResourceType = typeof(Labels))]
        public float MaxCaps { get; set; }

        [Range(0.01F, Domain.Types.Rules.Plans.Chat.MaxAps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxAps_Invalid")]
        [Display(Name = "MaxAps", ResourceType = typeof(Labels))]
        public float MaxChatAps { get; set; }

        [Range(0.01F, Domain.Types.Rules.Plans.Email.MaxAps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxAps_Invalid")]
        [Display(Name = "MaxAps", ResourceType = typeof(Labels))]
        public float MaxEmailAps { get; set; }

        [Range(0.01F, Domain.Types.Rules.Plans.Sms.MaxAps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxAps_Invalid")]
        [Display(Name = "MaxAps", ResourceType = typeof(Labels))]
        public float MaxSmsAps { get; set; }

        [Display(Name = "NotificationEmail", ResourceType = typeof(Labels))]
        [EmailDelimited(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Email_Invalid")]
        [StringLength(512)]
        public string NotificationEmail { get; set; }

        [Display(Name = "ReportEmailDomainWhitelist", ResourceType = typeof(Labels))]
        public string ReportEmailDomainWhitelist { get; set; }

        [Display(Name = "AllowBlankNumbers", ResourceType = typeof(Labels))]
        public bool AllowBlankNumbers { get; set; }

        // ReSharper disable once InconsistentNaming
        public bool MOSEnabled { get; set; }

        public bool OutboundEnabled { get; set; }

        public int? AccountId { get; set; }

        [Display(Name = "ProvisionedNumbers", ResourceType = typeof(Labels))]
        [StringLength(Constants.MaxStringLength)]
        public string ProvisionedNumbers { get; set; }

        [Display(Name = "ProvisionedDomains", ResourceType = typeof(Labels))]
        public IList<KeyViewData> ProvisionedDomains { get; set; }

        public string ProvisionedDomain { get; set; }

        [StringLength(Constants.MaxStringLength)]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Display(Name = "MobileWhitelist", ResourceType = typeof(Labels))]
        public string MobileWhitelist { get; set; }

        public string SmsProviderOptionsScript { get; set; }

        public SmsMobileViewData SmsMobileEdit { get; set; }

        [Display(Name = "SmsMobileNumbers", ResourceType = typeof(Resources.Labels))]
        public List<SmsMobileViewData> SmsMobiles { get; set; }

        [Display(Name = "BlockedNumbers", ResourceType = typeof(Labels))]
        [StringLength(Constants.MaxStringLength)]
        public string BlockedNumbers { get; set; }

        [Display(Name = "AllowedNumbers", ResourceType = typeof(Labels))]
        [StringLength(Constants.MaxStringLength)]
        public string AllowedNumbers { get; set; }

        [Display(Name = "TimeZone", ResourceType = typeof(Labels))]
        [Required(ErrorMessage = " ")]
        public int TimeZoneId { get; set; }

        [Display(Name = "AllowSpeechReplies", ResourceType = typeof(Labels))]
        public bool AllowSpeechReplies { get; set; }

        [Display(Name = "EnableCrawler", ResourceType = typeof(Labels))]
        public bool EnableCrawler { get; set; }

        [Required(ErrorMessage = " ")]
        [MinMaxSplit(Steps.Voice.MinConfidence, Steps.Voice.MaxConfidence, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "ThresholdConfidence_Invalid")]
        [MinMaxRange("GTE", Precision = 2, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "ThresholdMinorGreaterThanMajor_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Display(Name = "Default_Threshold_Confidence", ResourceType = typeof(Resources.Labels))]
        public string ConfidenceThreshold
        {
            get { return _confidenceThreshold; }
            set { _confidenceThreshold = value.FormatMinMaxSplit(2); }
        }

        [Required(ErrorMessage = " ")]
        [MinMaxSplit(Steps.Voice.MinMos, Steps.Voice.MaxMos, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "ThresholdMOS_Invalid")]
        [MinMaxRange("GTE", Precision = 2, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "ThresholdMinorGreaterThanMajor_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Display(Name = "Default_Threshold_MOS", ResourceType = typeof(Resources.Labels))]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1201
        // ReSharper disable once StyleCop.SA1306
        public string MOSThreshold
        {
            get { return _mosThreshold; }
            set { _mosThreshold = value.FormatMinMaxSplit(2); }
        }

        [Display(Name = "IdentityProvider", ResourceType = typeof(Labels))]
        public int? IdentityProviderId { get; set; }

        public bool UsesDefaultIdentityProvider => IdentityProviderId == null;

        public bool ShowIdentityProvider { get; set; }

        public IEnumerable<SelectListItem> IdentityProviderTypes { get; set; }

        // downstream only
        public List<SelectListItem> SmsProviders { get; set; }

        public List<SelectListItem> Countries { get; set; }

        public List<SelectListItem> Languages { get; set; }

        public List<SelectListItem> AsrPools { get; set; }

        public List<SelectListItem> StorageBackends { get; set; }

        public List<SelectListItem> Voices { get; set; }

        public List<SelectListItem> Timezones { get; set; }

        public IEnumerable<MediaTypePlansViewData> PlanTypes { get; set; }

        public List<SelectListItem> EnhancedConfidenceAlgorithmTypes { get; set; }

        public bool ReadOnly { get; set; }

        public string DatePattern { get; set; }

        public PaginatedView<PlanViewData> Plans { get; set; }

        // Dummy for the new mailbox dialog
        public EmailMailboxAccountViewData MailboxEdit { get; set; }

        [Display(Name = "EmailMailboxAccount", ResourceType = typeof(Resources.Labels))]
        public List<EmailMailboxAccountViewData> Mailboxes { get; set; }

        public IEnumerable<SelectListItem> Speakers { get; set; }

        public bool VelocityIsLicensed { get; set; }

        [Display(Name = "RunLoadCampaignWithoutBooking", ResourceType = typeof(Labels))]
        public bool RunLoadCampaignWithoutBooking { get; set; }

        [Display(Name = "AccountUsageReport", ResourceType = typeof(Labels))]
        public bool AllowAccountUsageReport { get; set; }

        [Display(Name = "EnableServices", ResourceType = typeof(Labels))]
        public bool EnableServices { get; set; }

        [Display(Name = "EnableIntegrationHub", ResourceType = typeof(Labels))]
        public bool EnableIntegrationHub { get; set; }

        public bool IntegrationHubLicensed { get; set; }

        [Display(Name = "EnableCxInsights", ResourceType = typeof(Labels))]
        public bool EnableCxInsights { get; set; }

        public bool CxInsightsLicensed { get; set; }

        [Display(Name = "EnableLiveVQ", ResourceType = typeof(Labels))]
        public bool EnableLiveVQ { get; set; }

        [StringLength(Constants.MaxStringLength)]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [RequiredIf("EnableLiveVQ", true)]
        [Shared.Web.DataAnnotations.Url(Strict = true, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Url_Invalid")]
        [Display(Name = "LiveVQPortalUrl", ResourceType = typeof(Labels))]
        public string LiveVQPortalUrl { get; set; }

        [StringLength(Constants.MaxStringLength)]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [RequiredIf("EnableLiveVQ", true)]
        [Display(Name = "LiveVQClientId", ResourceType = typeof(Labels))]
        public string LiveVQClientId { get; set; }

        public bool LiveVQLicensed { get; set; }

        [Display(Name = "EnableDynamicTranscription", ResourceType = typeof(Labels))]
        public bool EnableDynamicTranscription { get; set; }

        public bool DynamicTranscriptionLicensed { get; set; }

        public bool CyaraPhoneLicensed { get; set; }

        [Display(Name = "EnableCyaraPhone", ResourceType = typeof(Labels))]
        public bool EnableCyaraPhone { get; set; }

        [Display(Name = "EnhancedConfidenceAlgorithmType", ResourceType = typeof(Labels))]
        public string EnhancedConfidenceAlgorithmType { get; set; } = nameof(Cyara.Voice.Common.Types.Tags.EnhancedConfidenceAlgorithmType.Lenient);

        [Display(Name = "EnablePolqa", ResourceType = typeof(Labels))]
        public bool EnablePolqa { get; set; }

        [Display(Name = "Speaker", ResourceType = typeof(Labels))]
        public int? SpeakerId { get; set; }

        [Display(Name = "IgnoreCertificateErrors", ResourceType = typeof(Labels))]
        public bool IgnoreCertificateErrors { get; set; }

        public bool TfnSweepLicensed { get; set; }

        [Display(Name = "EnableTfnSweep", ResourceType = typeof(Labels))]
        public bool EnableTfnSweep { get; set; }

        public bool EndpointsLicensed { get; set; }

        [Display(Name = "EnableEndpoints", ResourceType = typeof(Labels))]
        public bool EnableEndpoints { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(ProvisionedDomain))
            {
                if (ProvisionedDomains == null)
                {
                    ProvisionedDomains = new List<KeyViewData>();
                }

                ProvisionedDomains.Add(new KeyViewData { Key = ProvisionedDomain });

                // empty them out in case they resubmit again and it fails.
                ProvisionedDomain = string.Empty;
            }

            if (ProvisionedDomains != null && ProvisionedDomains.Count > 0)
            {
                // remove all null keys
                ProvisionedDomains = ProvisionedDomains.Where(provisionedDomain => !string.IsNullOrEmpty(provisionedDomain.Key))
                    .ToList();

                for (var i = 0; i < ProvisionedDomains.Count; i++)
                {
                    IEnumerable<string> MemberName(int idx)
                    {
                        return new[] { "{0}[{1}].{2}".FormatWith(ReflectOn<AccountEditViewModel>.GetProperty(p => p.ProvisionedDomains).Name, idx, ReflectOn<KeyViewData>.GetProperty(p => p.Key).Name) };
                    }

                    var err = WhitelistDomain.Validate(ProvisionedDomains[i].Key);
                    if (err != null)
                    {
                        yield return new ValidationResult(ValidationMessage(err), MemberName(i));
                    }

                    if (ProvisionedDomains[i].Key.Length >= 512)
                    {
                        yield return new ValidationResult(ValidationMessages.Domain_WrongSize, MemberName(i));
                    }

                    var count = ProvisionedDomains.Count(
                        x => x.Key.Trim()
                                 .Equals(ProvisionedDomains[i].Key.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (count > 1)
                    {
                        yield return new ValidationResult(ValidationMessages.Domain_NotUnique, MemberName(i));
                    }
                }
            }

            if (SmsMobiles != null && SmsMobiles.Count > 0)
            {
                for (var i = 0; i < SmsMobiles.Count; i++)
                {
                    var count = SmsMobiles.Count(
                        x => x.Number.Trim()
                                 .Equals(SmsMobiles[i].Number.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (count > 1)
                    {
                        yield return new ValidationResult(
                            ValidationMessages.SmsNumber_NotUnique,
                            new[] { $"{ReflectOn<AccountEditViewModel>.GetProperty(p => p.SmsMobiles).Name}[{i}].{ReflectOn<SmsMobileViewData>.GetProperty(p => p.Number).Name}" });
                    }
                }
            }

            if (OutboundEnabled && !NumberCollection.IsValid(ProvisionedNumbers))
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.ProvisionedNumbers_Invalid,
                        new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.ProvisionedNumbers).Name });
            }

            if (!string.IsNullOrEmpty(BlockedNumbers) && !NumberCollection.IsValid(BlockedNumbers))
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.BlockedNumbers_Invalid,
                        new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.BlockedNumbers).Name });
            }

            if (!string.IsNullOrEmpty(AllowedNumbers) && !NumberCollection.IsValid(AllowedNumbers))
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.AllowedNumbers_Invalid,
                        new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.AllowedNumbers).Name });
            }

            if (!string.IsNullOrEmpty(ReportEmailDomainWhitelist))
            {
                IList<WhitelistDomainInvalidException> domainErrors = null;
                try
                {
                    var validator = new DomainCollection(ReportEmailDomainWhitelist);
                    ReportEmailDomainWhitelist = validator.ToString();
                }
                catch (DomainCollectionInvalidException ex)
                {
                    domainErrors = ex.DomainErrors;
                }

                if (domainErrors?.Count > 0)
                {
                    foreach (var err in domainErrors)
                    {
                        yield return this.ValidationError(ValidationMessage(err)).On(x => x.ReportEmailDomainWhitelist);
                    }
                }
            }

            SessionFacade session = new SessionFacade(HttpContextFactory.Current);
            IAccountService accountService = DependencyResolver.Current.GetService<IAccountService>();
            GenericResponse<Account> response;

            if (AccountId.HasValue && NeverExpires == false)
            {
                response = accountService.AccountGet(new GenericRequest<int>(AccountId.Value) { User = session.User });
                response.ExceptionIfError();

                // convert the expiry provided by user to the UTC for comparison purposes
                DateTime? expiryInUtc = Expiry;
                if (expiryInUtc != null)
                {
                    expiryInUtc = expiryInUtc.Value.FromLocal(session.UserTimezone);
                }

                if (response.Value.Plans.Any(x => x.EndDate > expiryInUtc))
                {
                    yield return new ValidationResult(
                        ValidationMessages.Expiry_BeforePlan,
                        new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.Expiry).Name });
                }
            }

            response = accountService.AccountGet(new GenericRequest<string>(Name) { User = session.User });
            response.ExceptionIfError();
            if (response.Value != null && (!AccountId.HasValue || AccountId.Value != response.Value.AccountId))
            {
                yield return new ValidationResult(
                    ValidationMessages.AccountName_Duplicate,
                    new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.Name).Name });
            }

            if (Mailboxes != null && Mailboxes.Count > 1)
            {
                foreach (IGrouping<string, EmailMailboxAccountViewData> duplicated in Mailboxes.GroupBy(x => x.EmailAddress).Where(x => x.Count() > 1))
                {
                    yield return new ValidationResult(
                        ValidationMessages.EmailAddress_Duplicate.FormatWith(duplicated.Key),
                        new[] { ReflectOn<AccountEditViewModel>.GetProperty(p => p.Mailboxes).Name });
                }
            }
        }

        private static string ValidationMessage(WhitelistDomainInvalidException err)
        {
            if (err == null)
            {
                return null;
            }

            switch (err.Error)
            {
                case WhitelistDomainError.BeginsWithDot:
                    return ValidationMessages.Domain_InvalidDotAtStart.FormatWith(err.Domain);

                case WhitelistDomainError.WildcardInvalid:
                    return ValidationMessages.Domain_InvalidWildcardAfterStart.FormatWith(err.Domain);

                // case WhitelistDomainError.InvalidCharacters:
                // case WhitelistDomainError.NullOrWhitespace:
                default:
                    return ValidationMessages.Domain_Invalid.FormatWith(err.Domain);
            }
        }
    }
}
