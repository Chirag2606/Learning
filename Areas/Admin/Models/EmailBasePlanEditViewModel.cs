namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Rules;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Web.Resources;

    public class EmailBasePlanEditViewModel : PlanEditViewModel
    {
        [Required(ErrorMessage = " ")]
        [RegularExpression("([0-9]+)", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Numeric")]
        [Range(1, Plans.Chat.MaxSessions, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Range")]
        [Display(Name = "Sessions", ResourceType = typeof(Labels))]
        public int Sessions { get; set; }

        [Required(ErrorMessage = " ")]
        [Range(0.01F, Plans.Email.MaxAps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxAps_Invalid")]
        [Display(Name = "MaxAps", ResourceType = typeof(Labels))]
        public float MaxEmailAps { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (ValidationResult result in base.Validate(validationContext))
            {
                yield return result;
            }
        
            SessionFacade session = new SessionFacade(HttpContextFactory.Current);

            IAccountService accountService = DependencyResolver.Current.GetService<IAccountService>();
            GenericResponse<Account> response = accountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();
            if (MaxEmailAps > response.Value.MaxEmailAps)
            {
                yield return new ValidationResult(ValidationMessages.Plan_MaxApsGreaterThanAccount, new[] { ReflectOn<EmailBasePlanEditViewModel>.GetProperty(p => p.MaxEmailAps).Name });
            }
        }
    }
}
