namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.ComponentModel.DataAnnotations;

    using Cyara.Shared.Web.Validation;

    public class ServerViewData
    {
        public int ServerId { get; set; }

        [Display(Name = "ServerName", ResourceType = typeof(Resources.Labels))]
        [Required(ErrorMessage = " ")]
        [StringLength(255, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "TServer_Length")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string ServerName { get; set; }

        [Display(Name = "Hostname", ResourceType = typeof(Resources.Labels))]
        [Required(ErrorMessage = " ")]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z-_.0-9:,]{0,63}$", ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Hostname_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string Hostname { get; set; }

        [Display(Name = "Port", ResourceType = typeof(Resources.Labels))]
        [Required(ErrorMessage = " ")]
        [Range(1, 65535, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Port_Invalid")]
        public int Port { get; set; }
    }
}