namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.Globalization;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Portal.Areas.Report.Models;

    public static class ScheduleDetailWithEndRepeatViewDataExtensions
    {
        public static void Prime(this ScheduleDetailWithEndRepeatViewData value, bool applyDefaults)
        {
            if (applyDefaults)
            {
                value.EndRepeat = EndRepeatType.Never;

                value.EndRepeatValue = null;

                value.EndRepeatDate = null;
            }

            value.DatePattern = DateTimeFormat.GetPickerDateFormat(MvcApplication.Settings);

            value.EndRepeatOptions =
                    EnumHelper.EnumToList(typeof(EndRepeatType), "EndRepeatType", sort: false)
                                 .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2, Selected = (EndRepeatType)Enum.Parse(typeof(EndRepeatType), x.Item1) == value.EndRepeat })
                                 .ToList();
        }

        public static string ToText(this ScheduleDetailWithEndRepeatViewData value, string multipleOccurencesTemplate = null)
        {
            switch (value.EndRepeat)
            {
                case EndRepeatType.Never:
                    break;
                case EndRepeatType.After:
                    if (value.EndRepeatValue.HasValue)
                    {
                        if (value.EndRepeatValue.Value == 1)
                        {
                            return Resources.Common.Schedule_Once;
                        }

                        if (value.EndRepeatValue.Value > 1)
                        {
                            if (string.IsNullOrEmpty(multipleOccurencesTemplate))
                            {
                                return Resources.Common.Schedule_ManyTimes.FormatWith(value.EndRepeatValue.Value);
                            }

                            return multipleOccurencesTemplate.FormatWith(value.EndRepeatValue.Value);
                        }
                    }

                    break;
                case EndRepeatType.On:
                    if (value.EndRepeatDate.HasValue)
                    {
                        return Resources.Common.Schedule_Until.FormatWith(value.EndRepeatDate.Value.FormatToDate());
                    }

                    break;
            }

            return string.Empty;
        }
    }
}
