namespace Cyara.Web.Portal.Areas.Api.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        /*
         * NOTE: This is a huge hack... we want to use the ISO local-utc format when handling dates... 
         * but there is no conversion of time, it just says, "hey this was a local time".  As such, when dates come back
         * from the database as proper UTC dates this will most likely need to be changed...
         */
        public static DateTime SetLocalKind(this DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                       ? DateTime.SpecifyKind(value, DateTimeKind.Local)
                       : value.ToLocalTime();
        }

        public static DateTime? SetLocalKind(this DateTime? value)
        {
            if (value == null)
            {
                return value;
            }

            return value.Value.Kind == DateTimeKind.Unspecified
                       ? DateTime.SpecifyKind(value.Value, DateTimeKind.Local)
                       : value.Value.ToLocalTime();
        }
    }
}