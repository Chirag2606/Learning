namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Resources;

    public class TemplateViewModel
    {
        [RangeIf("CreateMode", false, 1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "NotificationTemplate_NotFound")]
        public int TemplateId { get; set; }

        public bool CreateMode { get; set; }

        [Display(Name = "EmailTemplateName", ResourceType = typeof(Labels))]
        [Required(ErrorMessage = " ")]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [StringLength(100, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "NotificationTemplateName_WrongSize")]
        public string TemplateName { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Labels))]
        [Required(ErrorMessage = " ")]
        [StringLength(100, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Subject_WrongSize")]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string Subject { get; set; }

        [Display(Name = "MessageBody", ResourceType = typeof(Labels))]
        [Required(ErrorMessage = " ")]
        [AllowHtml]
        public string MessageBody { get; set; }

        public string TagsJson { get; set; }
    }
}