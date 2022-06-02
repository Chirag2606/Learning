namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Identity.ActiveDirectory;
    using Cyara.Web.Portal.Models;

    public class UserEditViewModel : ChangePasswordBaseViewModel, IValidatableObject
    {
        public string Action { get; set; }

        public bool EditMode { get; set; }

        public bool CanEditEmail { get; set; }

        [Display(Name = "ConfirmedEmail", ResourceType = typeof(Resources.Labels))]
        public bool? EmailConfirmed { get; set; }

        public bool IsLockedOut { get; set; }

        public bool IsEligibleForMobileAppInvite { get; set; }

        public string MobileAppDeepLink { get; set; }

        public string MobileAppError { get; set; }

        public bool Active { get; set; }

        public Guid UserId { get; set; }

        [Required(ErrorMessage = " ")]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Username_Length", MinimumLength = 3)]
        [RegularExpression(@"[^:]+", ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Username_InvalidCharacters")]
        [Display(Name = "Username", ResourceType = typeof(Resources.Labels))]
        public string Username { get; set; }

        [Display(Name = "AccountLevelRoles", ResourceType = typeof(Resources.Labels))]
        public IList<UserRoleViewData> AccountLevelRoles { get; set; }

        [Display(Name = "LegacyAccountLevelRoles", ResourceType = typeof(Resources.Labels))]
        public IList<UserRoleViewData> LegacyAccountLevelRoles { get; set; }

        [Display(Name = "PlatformLevelRoles", ResourceType = typeof(Resources.Labels))]
        public IList<UserRoleViewData> PlatformLevelRoles { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "FirstName", ResourceType = typeof(Resources.Labels))]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "LastName", ResourceType = typeof(Resources.Labels))]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Telephone", ResourceType = typeof(Resources.Labels))]
        [StringLength(50)]
        [Required(ErrorMessage = " ")]
        public string Telephone { get; set; }

        [Display(Name = "Mobile", ResourceType = typeof(Resources.Labels))]
        [StringLength(50)]
        public string Mobile { get; set; }

        [Required(ErrorMessage = " ")]
        [Display(Name = "Email", ResourceType = typeof(Resources.Labels))]
        [Email(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Email_Invalid")]
        [StringLength(256)]
        public string Email { get; set; }

        /// <summary>
        /// List of accounts allocated to this user (as in company accounts) - JSON string of array of IdNamePair instances
        /// </summary>
        public string Accounts { get; set; }

        /// <summary>
        /// Flag to say if user has edited accounts on the page or not
        /// </summary>
        public bool AccountsChanged { get; set; }

        [Display(Name = "LoginProvider", ResourceType = typeof(Resources.Labels))]
        public LoginProviders LoginProvider { get; set; }

        public NotificationsViewData Notifications { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LoginProvider.Has(LoginProviders.ActiveDirectory))
            {
                var settings = DependencyResolver.Current.GetService<MembershipProviderSettings>();
                var logger = DependencyResolver.Current.GetService<ILogger>();

                var aduser = new ADUser(logger, Username, settings);

                var result = aduser.Search();

                if (!result.Exists)
                {
                    yield return new ValidationResult(
                        Resources.Messages.ADUser_NotFound,
                        new[] { ReflectOn<UserEditViewModel>.GetProperty(p => p.Username).Name });
                }
            }

            if (PlatformLevelRoles?.Any() ?? false)
            {
                yield return new ValidationResult(
                    Resources.ValidationMessages.Roles_Invalid,
                    new[] { ReflectOn<UserEditViewModel>.GetProperty(p => p.PlatformLevelRoles).Name });
            }
        }
    }
}