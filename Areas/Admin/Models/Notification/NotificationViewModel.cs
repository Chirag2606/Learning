namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Cyara.Shared.Web.Models;

    public class NotificationViewModel : BaseViewModel, ITabbedView, IValidatableObject
    {
        public NotificationViewModel()
        {
            EmailLogs = new EmailLogsViewData();

            Templates = new TemplatesViewData();
        }

        public string SelectedTab { get; set; }

        public NewMessageViewData NewMessage { get; set; }

        public EmailLogsViewData EmailLogs { get; set; }

        public TemplatesViewData Templates { get; set; }

        public int MaximumAttachmentSizeInBytes { get; set; }

        // comma-separated list of accepted extensions
        public string AcceptedAttachmentFileTypes { get; set; }

        /// <summary>
        /// Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        /// A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The validation context.</param>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var vm in NewMessage.Validate(validationContext))
            {
                yield return vm;
            }
        }
    }
}