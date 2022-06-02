namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class PauseTimeType
    {
        public static PauseTimeType From(Shared.Types.TestCase.IStep step)
        {
            return new PauseTimeType
            {
                Max = (decimal)step.MaxPause,
                Min = (decimal)step.MinPause
            };
        }
    }
}