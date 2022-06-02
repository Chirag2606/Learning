namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TimeUnitSeconds
    {
        public static TimeUnitSeconds From(System.TimeSpan? span)
        {
            return span.HasValue 
                ? new TimeUnitSeconds { Seconds = (int)span.Value.TotalSeconds } 
                : new TimeUnitSeconds { Seconds = 0 };
        }
    }
}