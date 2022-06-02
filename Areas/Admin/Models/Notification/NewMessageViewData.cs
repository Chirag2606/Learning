namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Resources;

    public class NewMessageViewData : IValidatableObject
    {
        public NewMessageViewData()
        {
            Accounts = Enumerable.Empty<AccountInfoViewData>();
            Attachments = (new string[0]).ToJson();
        }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Labels))]
        [Required(ErrorMessage = " ")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Subject_WrongSize")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string Subject { get; set; }

        [Display(Name = "MessageBody", ResourceType = typeof(Resources.Labels))]
        [Required(ErrorMessage = " ")]
        [AllowHtml]
        public string MessageBody { get; set; }

        /// <summary>
        /// These are selected accounts for submission
        /// </summary>
        public int[] SelectedAccounts { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "SomeAccountsMustBeSelected")] 
        public string SendingSummary { get; set; }

        public string TagsJson { get; set; }

        public string NotificationStatistics { get; set; }

        public IEnumerable<AccountInfoViewData> Accounts { get; set; }

        public MessageViewData Message { get; set; }

        public string Attachments { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedAccounts == null || SelectedAccounts.Length == 0)
            {
                var memberNames = new[]
                {
                    ReflectOn<NewMessageViewData>.GetProperty(p => p.SelectedAccounts).Name,
                    ReflectOn<NewMessageViewData>.GetProperty(p => p.SendingSummary).Name
                };

                yield return new ValidationResult(ValidationMessages.SomeAccountsMustBeSelected, memberNames);
            }
        }
    }
}
