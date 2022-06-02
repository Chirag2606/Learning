namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using Cyara.Shared.Types.Campaign;

    public partial class RunFor
    {
        public static RunFor From(ICampaign campaign)
        {
            RunFor val = new RunFor();
            
            if (campaign.TotalCallsToMake.HasValue)
            {
                val.Value = campaign.TotalCallsToMake.Value;
                val.Unit = RunForUnit.Calls;
            }
            else if (campaign.RunTime.HasValue)
            {
                val.Value = (int)campaign.RunTime.Value.TotalMinutes;
                val.Unit = RunForUnit.Minutes;
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