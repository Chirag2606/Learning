namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.ComponentModel.DataAnnotations;

    using Cyara.Web.Resources;

    public class IntHubConfigurationViewModel
    {
        [Display(Name = "IntegrationHubUrl", ResourceType = typeof(Labels)),
         Shared.Web.DataAnnotations.Url(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Url_Invalid")]
        public string IntegrationHubUrl { get; set; }

        [Display(Name = "ScopeSecret", ResourceType = typeof(Labels))]
        public string[] ScopeSecret { get; set; }
    }
}