namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Core.Validation;
    using Cyara.Web.Resources;
    using Shared.Types.Sms;

    public class SmsMobileViewData
    {
        public int SmsNumberId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MobileNumber_Required")]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [StringLength(Cyara.Domain.Types.Rules.Constants.Sms.MobilePhoneLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "WrongSize")]
        [Shared.Web.TestCaseValidation.InternationalPhoneNumberValidator(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MobileNumber_Invalid")]
        [Display(Name = "TableHeading_Mobile", ResourceType = typeof(Common))]
        public string Number { get; set; }

        [Display(Name = "TableHeading_Provider", ResourceType = typeof(Common))]
        public SmsGatewayProvider SmsGatewayProvider { get; set; }

        public IEnumerable<SelectListItem> Providers { get; set; }

        [RequiredIf("SmsGatewayProvider", SmsGatewayProvider.Smpp, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "SmppServer_Required")]
        [Display(Name = "TableHeading_Server", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "SmppServer_WrongSize")]
        public string SmppServer { get; set; }

        [RequiredIf("SmsGatewayProvider", SmsGatewayProvider.Smpp, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "SmppPort_Required")]
        [Display(Name = "TableHeading_Port", ResourceType = typeof(Common))]
        [Range(1, 65535)]
        public string SmppPort { get; set; }

        [RequiredIf("SmsGatewayProvider", SmsGatewayProvider.Smpp, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Password_Required")]
        [Display(Name = "TableHeading_Password", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "SmppServerPassword_WrongSize")]
        public string Password { get; set; }

        [RequiredIf("SmsGatewayProvider", SmsGatewayProvider.Smpp, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Username_Required")]
        [Display(Name = "TableHeading_Username", ResourceType = typeof(Common))]
        [StringLength(512, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "SmppServerUsername_WrongSize")]
        public string Username { get; set; }
    }
}