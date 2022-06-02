namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Cyara.Domain.Types.Common;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Models;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Portal.Models.SchedulerStatistics;

    public class AccountPortUsageViewModel : BaseViewModel, IMediaTypeDiscriminator, ITabbedView, IValidatableObject
    {
        public string CurrentTab { get; set; }

        public string DatePattern { get; set; }

        public MediaType MediaType { get; set; }

        public RealTimePortUsageWebSettings RealTimePortUsageWebSettings { get; set; }

        public AccountPeakPortsTabData PeakPorts { get; set; }

        public string SelectedTab { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return null;
        }
    }
}