namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class ThresholdTimeType
    {
        public static ThresholdTimeType From(IStep step)
        {
            return AutoMapper.Mapper.Map<V2_2.ThresholdTimeType, ThresholdTimeType>(V2_2.ThresholdTimeType.From(step));
        }

        public static ThresholdTimeType From(TestStepResultReport step)
        {
            return AutoMapper.Mapper.Map<V2_2.ThresholdTimeType, ThresholdTimeType>(V2_2.ThresholdTimeType.From(step));
        }
    }
}