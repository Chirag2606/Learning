namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System.Threading.Tasks;

    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;

    using MediatR;

    public static class CustomReportViewModelExtensions
    {
        public static async Task<CustomReportViewModel> Prime(this CustomReportViewModel model, int accountId, IMediator mediator, PaginatedView paging, bool applyDefaults)
        {
            if (applyDefaults)
            {
                paging = new PaginatedView
                             {
                                 PageNumber = 1,
                                 PageSize = MvcApplication.Settings.DefaultPageSize,
                                 SortAscending = true,
                                 SortColumn = Columns.Name
                             };
            }

            var query = new CustomReportPaginatedQuery
            {
                AccountId = accountId,
                PageSize = paging.PageSize,
                PageNo = paging.PageNumber,
                SortAscending = paging.SortAscending,
                SortField = paging.SortColumn
            };

            var response = await mediator.Send(query);

            model.Reports = new PaginatedView<CustomReportViewData>();
            model.Reports.Prime(response, query.SortAscending, query.SortField, query.PageNo);
            model.Reports.FromPaginatedResponse(response);

            return model;
        }
    }
}