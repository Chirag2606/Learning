namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    public partial class TimeUnitSeconds
    {
        public static TimeUnitSeconds From(System.TimeSpan? span)
        {
            if (span.HasValue)
            {
                return new TimeUnitSeconds { Seconds = (int)span.Value.TotalSeconds };
            }

            return new TimeUnitSeconds { Seconds = 0 };
        }
    }
}