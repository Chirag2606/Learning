namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Portal.Areas.Report.Models;

    public static class CustomReportEntityExtensions
    {
        public static ReportScheduleViewData ToReportScheduleViewData(this CustomReportEntity value)
        {
            switch (value.GetStatus())
            {
                case ScheduleStatusEnum.NonExistent:
                    return new ReportScheduleViewData
                               {
                                   IsScheduled = false,
                                   ScheduleLabel = Resources.Common.ScheduleCreate
                               };

                case ScheduleStatusEnum.Disabled:
                    return new ReportScheduleViewData
                               {
                                   IsScheduled = false,
                                   ScheduleLabel = Resources.Common.ScheduleDisabled
                               };

                case ScheduleStatusEnum.Active:
                    var schedule = value.GetSchedule();
                    var scheduleVm = Mapper.Map<Schedule, ScheduleViewModel>(schedule);
                    var label = scheduleVm.Period != null ? scheduleVm.Period.ToString() : Resources.Common.Schedule_Once;

                    return new ReportScheduleViewData { IsScheduled = true, ScheduleLabel = label };

                case ScheduleStatusEnum.Expired:
                    return new ReportScheduleViewData
                               {
                                   IsScheduled = false,
                                   ScheduleLabel = Resources.Common.ScheduleExpired
                               };
                default:
                    throw new Exception("Unrecognised ScheduleStatus value: {0}".FormatWith(value.GetStatus()));
            }
        }

        public static ScheduleStatusEnum GetStatus(this CustomReportEntity value)
        {
            var schedule = value.GetSchedule();

            if (schedule == null)
            {
                return ScheduleStatusEnum.NonExistent;
            }

            if (value.NextRun != null)
            {
                return ScheduleStatusEnum.Active;
            }
            
            if (schedule.Active)
            {
                return ScheduleStatusEnum.Expired;
            }
            
            return ScheduleStatusEnum.Disabled;
        }
    }
}
