namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System.Threading.Tasks;

    using Cyara.Domain.Types.Common;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Globalization;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Models.SchedulerStatistics;

    using MediatR;

    public static class AccountPortUsageViewModelExtensions
    {
        public static async Task<AccountPortUsageViewModel> Prime(
            this AccountPortUsageViewModel value,
            RealTimePortUsageWebSettings settings,
            IMediator mediator,
            MediaType mediaType,
            bool isModelValid,
            SessionFacade session,
            IAuthorisationManager authorisationManager,
            RestApiFacade webApi,
            bool applyDefaults)
        {
            if (applyDefaults)
            {
                value.SelectedTab = "pagePorts";
                value.CurrentTab = "Campaign";
            }

            value.CurrentTab = value.CurrentTab ?? "Campaign";

            value.RealTimePortUsageWebSettings = settings;

            value.PeakPorts = value.PeakPorts ?? new AccountPeakPortsTabData();
            await value.PeakPorts.PrimeAsync(webApi, session.UserTimezone, session.SelectedAccount.Id, applyDefaults, isModelValid);

            value.DatePattern = DateTimeFormat.GetPickerDateFormat(MvcApplication.Settings);

            return value;
        }
    }
}