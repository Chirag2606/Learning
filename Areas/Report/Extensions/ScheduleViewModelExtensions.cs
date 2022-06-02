namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Globalization;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;

    using MediatR;

    public static class ScheduleViewModelExtensions
    {
        private static readonly Dictionary<RepeatOption, Func<ISchedulePeriod, bool, ISchedulePeriod>> Primers =
            new Dictionary<RepeatOption, Func<ISchedulePeriod, bool, ISchedulePeriod>>
                {
                    { RepeatOption.None, (model, applyDefaults) => (ISchedulePeriod)null },
                    { RepeatOption.Hourly, (model, applyDefaults) => ((ScheduleHourlyViewData)model).Prime(applyDefaults) },
                    { RepeatOption.Daily, (model, applyDefaults) => ((ScheduleDailyViewData)model).Prime(applyDefaults) },
                    { RepeatOption.Weekly, (model, applyDefaults) => ((ScheduleWeeklyViewData)model).Prime(applyDefaults) },
                    { RepeatOption.Monthly, (model, applyDefaults) => ((ScheduleMonthlyViewData)model).Prime(applyDefaults) },
                    { RepeatOption.Yearly, (model, applyDefaults) => ((ScheduleYearlyViewData)model).Prime(applyDefaults) }
                };

        private static readonly Dictionary<RepeatOption, Func<ISchedulePeriod>> Factory =
            new Dictionary<RepeatOption, Func<ISchedulePeriod>>
                {
                    { RepeatOption.None, () => (ISchedulePeriod)null },
                    { RepeatOption.Hourly, () => new ScheduleHourlyViewData() },
                    { RepeatOption.Daily, () => new ScheduleDailyViewData() },
                    { RepeatOption.Weekly, () => new ScheduleWeeklyViewData() },
                    { RepeatOption.Monthly, () => new ScheduleMonthlyViewData() },
                    { RepeatOption.Yearly, () => new ScheduleYearlyViewData() }
                };

        public static ScheduleViewModel Prime(this ScheduleViewModel value,  int accountId, bool applyDefaults)
        {
            if (applyDefaults)
            {
                value.Repeats = RepeatOption.None;
                value.Period = null;
                value.Start = DateTime.UtcNow.ToUserLocal();
                value.Active = true;
                value.Period = Factory[value.Repeats]();
            }

            value.RepeatChoices = EnumHelper.EnumToList(typeof(RepeatOption), "RepeatOption", sort: false)
                .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 });

            // TODO: Ignoring Pdf until implemented
            value.FormatChoices = EnumHelper.EnumToList(typeof(CustomReportExportType), "CustomReportExportType", sort: false)
                .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 });
            
            value.DatePattern = DateTimeFormat.GetPickerDateTimeFormat(MvcApplication.Settings);

            value.SelectedAccountId = accountId;
            
            Primers[value.Repeats](value.Period, value.IsReloading || applyDefaults);
            
            value.IsReloading = false;
            value.IsRefreshing = false;
            value.RefreshCounterId = 0;

            return value;
        }

        public static async Task<ScheduleViewModel> PrimeEdit(this ScheduleViewModel model, IMediator mediator, int accountId, int reportId)
        {
            model.ReportId = reportId;
            
            var customReportRequest = new CustomReportGetQuery
                                          {
                                              AccountId = accountId,
                                              ReportId = reportId
                                          };

            var customReport = await mediator.Send(customReportRequest);
            var schedule = customReport.GetSchedule();
            if (schedule != null)
            {
                Mapper.Map(schedule, model);
                model.Prime(accountId, false);
            }
            else
            {
                model.Prime(accountId, true);
            }
            
            return model;
        }
    }
}