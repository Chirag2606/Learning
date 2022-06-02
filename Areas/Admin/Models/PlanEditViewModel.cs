namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Domain.Types.TestCase;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Core.Validation;
    using Cyara.Web.Resources;

    [Flags]
    public enum RequiredFields
    {
        None = 0,
        SubType = 0x1,
        NotificationList = 0x2,
    }

    public class PlanEditViewModel : BaseViewModel, IValidatableObject
    {
        public int? PlanId { get; set; }

        [Display(Name = "Type", ResourceType = typeof(Labels))]
        public string Plan { get; set; }

        public PlanType PlanType { get; set; }

        [Display(Name = "MediaType", ResourceType = typeof(Labels))]
        public MediaType MediaType { get; set; }

        [Display(Name = "SubType", ResourceType = typeof(Labels))]
        public ExchangeType SubType { get; set; }

        public RequiredFields Requires { get; set; }

        public string DatePattern { get; set; }

        public bool ReadOnly { get; set; }

        [Required(ErrorMessage = " ")]
        [StringLength(128, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "PlanName_WrongSize")]
        [Display(Name = "PlanName", ResourceType = typeof(Labels))]
        public string Name { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "PlanNotes_WrongSize")]
        [Display(Name = "Notes", ResourceType = typeof(Labels))]
        public string Notes { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "StartDate", ResourceType = typeof(Labels))]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime? Start { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "ExpiryDate", ResourceType = typeof(Labels))]
        [GreaterThan("Start", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ExpiryDate_BeforeStartDate")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime? Expiry { get; set; }

        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "NotificationEmail_WrongSize")]
        [Display(Name = "NotificationEmail", ResourceType = typeof(Labels))]
        [EmailDelimited(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Email_Invalid")]
        public string NotificationEmail { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var session = new SessionFacade(HttpContextFactory.Current);

            var accountService = DependencyResolver.Current.GetService<IAccountService>();
            var response = accountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            var planResponse = accountService.PlanGet(new AccountRequest<string>(this.Name) { AccountId = session.SelectedAccount.Id, User = session.User });

            planResponse.ExceptionIfError();

            if (planResponse.Value != null && planResponse.Value.PlanId != this.PlanId)
            {
                yield return new ValidationResult(
                    ValidationMessages.PlanName_Duplicate,
                    new[] { ReflectOn<PlanEditViewModel>.GetProperty(p => p.Name).Name });
            }

            // convert account expiry to user local timezone for comparison purposes
            DateTime? accountExpiryInUsersTimezone = response.Value.Expiry;
            if (accountExpiryInUsersTimezone != null)
            {
                accountExpiryInUsersTimezone = accountExpiryInUsersTimezone.Value.ToUserLocal();
            }

            if (accountExpiryInUsersTimezone.HasValue && Expiry.HasValue && accountExpiryInUsersTimezone.Value < Expiry)
            {
                yield return new ValidationResult(
                    ValidationMessages.Plan_ExpiresAfterTheAccount,
                    new[] { ReflectOn<PlanEditViewModel>.GetProperty(p => p.Expiry).Name });
            }
        }
    }
}