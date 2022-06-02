namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Identity.ActiveDirectory;

    public class SuperUserEditViewModel : UserEditViewModel
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LoginProvider.Has(LoginProviders.ActiveDirectory))
            {
                var settings = DependencyResolver.Current.GetService<MembershipProviderSettings>();
                var logger = DependencyResolver.Current.GetService<ILogger>();
                var aduser = new ADUser(logger, Username, settings);

                var result = aduser.Search();
                if (!result.Exists)
                {
                    yield return
                        new ValidationResult(
                            Resources.Messages.ADUser_NotFound,
                            new[] { ReflectOn<UserEditViewModel>.GetProperty(p => p.Username).Name });
                }
            }

            if (LoginProvider.HasFlag(LoginProviders.Google))
            {
                var validator = new EmailAttribute();
                if (!validator.IsValid(Username))
                {
                    yield return new ValidationResult(
                        Resources.Messages.User_NameShouldBeEmail,
                        new[] { ReflectOn<UserEditViewModel>.GetProperty(p => p.Username).Name });
                }
            }
        }
    }
}