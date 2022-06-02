namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class ConfidenceType
    {
        public static ConfidenceType From(VoiceStep step)
        {
            return new ConfidenceType
            {
                Major = step.MajorConfidenceLevel,
                Minor = step.MinorConfidenceLevel
            };
        }

        public static ConfidenceType From(TestStepResultReport step)
        {
            return new ConfidenceType
            {
                Minor = step.MinorConfidenceLevel,
                Major = step.MajorConfidenceLevel
            };
        }
    }
}