namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net.Mail;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Responses;
    using Cyara.Shared.Collections;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Core.Validation;
    using Cyara.Web.Resources;

    [Serializable]
    public class ScheduleViewModel : BaseViewModel, IValidatableObject
    {
        public int ReportId { get; set; }

        public bool Active { get; set; }

        /// <summary>
        /// reloading is when user changes the schedule type dropdown (from monthly to yearly for example)
        /// </summary>
        public bool IsReloading { get; set; }

        /// <summary>
        /// refreshing is just getting the schedule period time description
        /// </summary>
        public bool IsRefreshing { get; set; }

        public int RefreshCounterId { get; set; }

        public string ReplaceTarget { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "ScheduleStartDate", ResourceType = typeof(Labels))]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime Start { get; set; }

        [Display(Name = "Repeats", ResourceType = typeof(Labels))]
        public RepeatOption Repeats { get; set; }

        public IEnumerable<SelectListItem> RepeatChoices { get; set; }

        [Display(Name = "Format", ResourceType = typeof(Labels))]
        public CustomReportExportType Format { get; set; }

        public IEnumerable<SelectListItem> FormatChoices { get; set; }

        [Required(ErrorMessage = " ")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [EmailDelimited(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Email_Invalid")]
        [Display(Name = "EmailRecipients", ResourceType = typeof(Labels))]
        public string EmailRecipients { get; set; }

        public string DatePattern { get; set; }

        [Display(Name = "Summary", ResourceType = typeof(Labels))]
        public ISchedulePeriod Period { get; set; }

        public bool IncludeFailed { get; set; }

        public bool IncludeAborted { get; set; }

        public bool IncludeSatisfactory { get; set; }

        public bool IncludeInternalErrors { get; set; }

        public bool IncludeTestRuns { get; set; }

        public bool CompressReports { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Period != null)
            {
                foreach (var vr in Period.Validate(validationContext))
                {
                    yield return vr;
                }
            }

            if (!string.IsNullOrWhiteSpace(EmailRecipients))
            {
                if (SelectedAccountId.HasValue)
                {
                    SessionFacade session = new SessionFacade(HttpContextFactory.Current);
                    IAccountService accountService = DependencyResolver.Current.GetService<IAccountService>();
                    GenericResponse<Account> response = accountService.AccountGet(new GenericRequest<int>(SelectedAccountId.Value) { User = session.User });
                    response.ExceptionIfError();

                    var results = EmailValidator.ValidateEmailsAgainstWhitelist(response.Value.Properties.ReportEmailDomainWhitelist, EmailRecipients, nameof(EmailRecipients));
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}