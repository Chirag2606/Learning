namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class RunFor
    {
        public static RunFor From(Cyara.Shared.Types.Campaign.ICampaign campaign)
        {
            var val = new RunFor();

            if (campaign.RunTime.HasValue)
            {
                val.Value = (int)campaign.RunTime.Value.TotalMinutes;
                val.Unit = RunForUnit.Minutes;
            }
            else if (campaign.TotalCallsToMake.HasValue)
            {
                val.Value = campaign.TotalCallsToMake.Value;
                val.Unit = RunForUnit.Calls;
            }
            else
            {
                val.Value = 0;
            }

            if (val.Value == 0)
            {
                val.Unit = RunForUnit.Calls;
            }

            return val;
        }
    }
}