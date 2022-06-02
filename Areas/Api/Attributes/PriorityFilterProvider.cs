namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Filters;

    public interface IFilterPriority
    {
        int Priority { get; set; }
    }

    public class PriorityFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(System.Web.Http.HttpConfiguration configuration, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
        {
            List<IFilter> actionFilters = actionDescriptor?.GetFilters().ToList() ?? new List<IFilter>();
            List<IFilter> controllerFilters = actionDescriptor?.ControllerDescriptor.GetFilters().ToList() ?? new List<IFilter>();

            var controllerPriorityActions = controllerFilters.Where(x => x is IFilterPriority).Select(x => new { Filter = x, ((IFilterPriority)x).Priority });
            var actionPriorityActions = actionFilters.Where(x => x is IFilterPriority).Select(x => new { Filter = x, ((IFilterPriority)x).Priority });

            // We consider that controller filters with priority are ordered first, then action filters with priority
            // then any other controller filters, then any other action filters
            // Priority Order is 1,2,3...n.. [undefined] .. -n...-3,-2,-1
            // NOTE: This order is reversed after the action is executed... ie a filter with the priorty 1 will be considered first upon request, and last upon response.
            List<FilterInfo> filters = new List<FilterInfo>();
            filters.AddRange(controllerPriorityActions.Where(x => x.Priority >= 0).OrderBy(x => x.Priority).Select(x => new FilterInfo(x.Filter, FilterScope.Controller)));
            filters.AddRange(actionPriorityActions.Where(x => x.Priority >= 0).OrderBy(x => x.Priority).Select(x => new FilterInfo(x.Filter, FilterScope.Controller)));
            filters.AddRange(controllerFilters.Where(x => !(x is IFilterPriority)).Select(x => new FilterInfo(x, FilterScope.Controller)));
            filters.AddRange(actionFilters.Where(x => !(x is IFilterPriority)).Select(x => new FilterInfo(x, FilterScope.Controller)));
            filters.AddRange(actionPriorityActions.Where(x => x.Priority < 0).OrderBy(x => x.Priority).Select(x => new FilterInfo(x.Filter, FilterScope.Controller)));
            filters.AddRange(controllerPriorityActions.Where(x => x.Priority < 0).OrderBy(x => x.Priority).Select(x => new FilterInfo(x.Filter, FilterScope.Controller)));

            return filters;
        }
    }
}