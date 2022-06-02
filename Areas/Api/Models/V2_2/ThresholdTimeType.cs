namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class ThresholdTimeType
    {
        public static ThresholdTimeType From(IStep step)
        {
            return new ThresholdTimeType
            {
                Major = (decimal)step.MajorThreshold,
                Minor = (decimal)step.MinorThreshold
            };
        }

        public static ThresholdTimeType From(TestStepResultReport step)
        {
            return new ThresholdTimeType
            {
                Major = (decimal)step.MajorThresholdTime,
                Minor = (decimal)step.MinorThresholdTime
            };
        }
    }
}