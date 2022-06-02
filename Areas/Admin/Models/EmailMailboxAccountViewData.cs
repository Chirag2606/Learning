namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.ComponentModel.DataAnnotations;

    using Cyara.Web.Resources;

    public class EmailMailboxAccountViewData
    {
        public int AccountId { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Email_Invalid")]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "EmailAddress_Required")]
        [Display(Name = "TableHeading_Email", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "EmailAddress_WrongSize")]
        public string EmailAddress { get; set; }

        public int MailboxAccountId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Password_Required")]
        [Display(Name = "TableHeading_Password", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MailboxPassword_WrongSize")]
        public string MailboxPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Username_Required")]
        [Display(Name = "TableHeading_Username", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MailboxUsername_WrongSize")]
        public string MailboxUsername { get; set; }

        /// <summary>
        /// NOTE: This should be populated from information about whether a test case uses the email address - but that is stored in JSON at the moment
        /// and is not worth fetching - this is a TODO item to remedy this situation, so that the link remains in-tact and we don't accidentally
        /// delete an email account that is used by an email test case step.
        /// </summary>
        public bool MailboxInUse { get; set; }
    }
}