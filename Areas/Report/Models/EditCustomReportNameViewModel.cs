namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Threading;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Resources;
    using MediatR;

    public class EditCustomReportNameViewModel : BaseViewModel, IValidatableObject
    {
        public int ReportId { get; set; }

        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "CustomReportName_Required")]
        [StringLength(200, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "CustomReportName_WrongSize")]
        public string ReportName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(ReportName))
            {
                var mediator = DependencyResolver.Current.GetService<IMediator>();

                var session = new SessionFacade(HttpContextFactory.Current);

                var customReport =
                    AsyncHelpers.RunSync(
                        () => mediator.Send(new CustomReportGetQuery { AccountId = session.SelectedAccount.Id, Name = ReportName }));

                if (customReport != null && customReport.ReportId != ReportId)
                {
                    yield return
                        new ValidationResult(
                            ValidationMessages.CustomReportName_Unique,
                            new[] { ReflectOn<EditCustomReportNameViewModel>.GetProperty(p => p.ReportName).Name });
                }
            }
        }
    }
}
