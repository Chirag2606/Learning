namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    public partial class TimeSpan
    {
        public static TimeSpan From(System.TimeSpan? span)
        {
            return AutoMapper.Mapper.Map<V2_2.TimeSpan, TimeSpan>(V2_2.TimeSpan.From(span));
        }

        public static TimeSpan FromSeconds(float seconds)
        {
            return AutoMapper.Mapper.Map<V2_2.TimeSpan, TimeSpan>(V2_2.TimeSpan.FromSeconds(seconds));
        }
    }
}