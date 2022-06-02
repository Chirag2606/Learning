namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    public partial class TimeSpan
    {
        public static TimeSpan From(System.TimeSpan? span)
        {
            if (span == null)
            {
                return null;
            }

            return new TimeSpan
            {
                Days = (int)span.Value.TotalDays,
                Hours = span.Value.Hours,
                Minutes = span.Value.Minutes,
                Seconds = span.Value.Seconds,
                Milliseconds = span.Value.Milliseconds
            };
        }

        public static TimeSpan FromSeconds(float seconds)
        {
            var span = System.TimeSpan.FromSeconds(seconds);

            return new TimeSpan
            {
                Days = (int)span.TotalDays,
                Hours = span.Hours,
                Minutes = span.Minutes,
                Seconds = span.Seconds,
                Milliseconds = span.Milliseconds
            };
        }
    }
}