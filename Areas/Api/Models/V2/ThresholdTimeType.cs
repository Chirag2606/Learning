namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class ThresholdTimeType
    {
        public static ThresholdTimeType From(Shared.Types.TestCase.IStep step)
        {
            return new ThresholdTimeType
            {
                Major = (decimal)step.MajorThreshold,
                Minor = (decimal)step.MinorThreshold
            };
        }
    }
}