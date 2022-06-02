namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Rules;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Web.Resources;

    public class SmsPlanBase : PlanEditViewModel
    {
        [Required(ErrorMessage = " ")]
        [RegularExpression("([0-9]+)", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Numeric")]
        [Range(1, Plans.Sms.MaxSessions, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Range")]
        [Display(Name = "Sessions", ResourceType = typeof(Labels))]
        public int Sessions { get; set; }

        [Required(ErrorMessage = " ")]
        [Range(0.01F, Plans.Sms.MaxAps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxAps_Invalid")]
        [Display(Name = "MaxAps", ResourceType = typeof(Labels))]
        public float MaxAps { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
            {
                yield return result;
            }

            var session = new SessionFacade(HttpContextFactory.Current);

            var accountService = DependencyResolver.Current.GetService<IAccountService>();
            var response = accountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            if (MaxAps > response.Value.MaxSmsAps)
            {
                yield return new ValidationResult(
                    ValidationMessages.Plan_MaxApsGreaterThanAccount,
                    new[] { ReflectOn<SmsPlanBase>.GetProperty(p => p.MaxAps).Name });
            }
        }
    }
}