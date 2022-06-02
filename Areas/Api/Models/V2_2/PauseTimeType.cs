namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class PauseTimeType
    {
        public static PauseTimeType From(IStep step)
        {
            return new PauseTimeType
            {
                Max = (decimal)step.MaxPause,
                Min = (decimal)step.MinPause
            };
        }

        public static PauseTimeType From(TestStepResultReport step)
        {
            return new PauseTimeType
            {
                Max = (decimal)step.MaxPauseTime,
                Min = (decimal)step.MinPauseTime
            };
        }
    }
}