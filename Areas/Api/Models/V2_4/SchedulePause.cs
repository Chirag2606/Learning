namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class SchedulePause
    {
        public static SchedulePause FromVal(Domain.Types.Shared.SchedulePause schedulePause, IDataHelper dataHelper)
        {
            return new SchedulePause
                   {
                       From = dataHelper.Output(schedulePause.PauseStart),
                       To = dataHelper.Output(schedulePause.PauseEnd),
                       Note = schedulePause.Note
                   };
        }
    }
}