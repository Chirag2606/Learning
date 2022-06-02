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

    public class VoicePlanBaseWithCaps : VoicePlanBase
    {
        [Required(ErrorMessage = " ")]
        [Range(0.01F, Plans.Voice.MaxCaps, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxCaps_Invalid")]
        [Display(Name = "MaxCaps", ResourceType = typeof(Labels))]
        public float MaxCaps { get; set; }

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

            if (MaxCaps > response.Value.MaxCaps)
            {
                yield return new ValidationResult(
                    ValidationMessages.Plan_MaxCapsGreaterThanAccount,
                    new[] { ReflectOn<VoicePlanBaseWithCaps>.GetProperty(p => p.MaxCaps).Name });
            }
        }
    }
}