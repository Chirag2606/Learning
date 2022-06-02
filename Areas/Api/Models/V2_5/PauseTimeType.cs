namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class PauseTimeType
    {
        public static PauseTimeType From(IStep step)
        {
            return AutoMapper.Mapper.Map<V2_2.PauseTimeType, PauseTimeType>(V2_2.PauseTimeType.From(step));
        }

        public static PauseTimeType From(TestStepResultReport step)
        {
            return AutoMapper.Mapper.Map<V2_2.PauseTimeType, PauseTimeType>(V2_2.PauseTimeType.From(step));
        }
    }
}