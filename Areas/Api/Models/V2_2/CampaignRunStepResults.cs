namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web.Http;

    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Models.Api.V1_0;

    public partial class CampaignRunStepResults
    {
        public static PaginatedResult<TestCaseResult2> Prime(NameValueCollection query, int collectionCount = -1)
        {
            var value = new PaginatedResult<TestCaseResult2>();

            int page = 1;
            int.TryParse(Query.GetValueByCaseInsensitiveKey(query, "page", string.Empty), out page);

            // ensure the page is at least one
            page = Math.Max(page, 1);

            int perPage;
            if (!int.TryParse(Query.GetValueByCaseInsensitiveKey(query, "perPage", string.Empty), out perPage))
            {
                perPage = 100;
            }

            // ensure the page size is less than the configured maximum and greater than one
            perPage = Math.Min(MvcApplication.Settings.ApiMaximumPageSize, Math.Max(perPage, 1));

            if (perPage >= 1000)
            {
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new System.Net.Http.StringContent(ApiMessageConstants.ValidationPageSizeOver1000),
                    ReasonPhrase = ApiMessageConstants.InvalidArgumentReason
                });
            }

            if (collectionCount >= 0)
            {
                var totalPages = (int)Math.Ceiling((double)collectionCount / perPage);
                if (page > totalPages)
                {
                    page = 1;
                }

                value.PageCount = totalPages;
            }
            
            value.Page = page;            
            value.PerPage = perPage;
            value.TotalCount = collectionCount;
            value.Query = query;

            return value;
        }

        public IEnumerable<TestCaseResult2> GetPage(PaginatedResult<TestCaseResult2> value)
        {
            return TestCaseResults.Skip((value.Page - 1) * value.PerPage)
                .Take(value.PerPage)
                .ToList();
        }

        public void GeneratePaginatedLinks(PaginatedResult<TestCaseResult2> value, ApiController context)
        {
            const string RouteName = "API_V2_0_Report_Filtered";

            var links = new List<object>();
            var routeValues = value.Query.AllKeys.ToDictionary(k => k.ToLower(), k => (object)value.Query[k]);

            routeValues.Add("controller", "run");
            
            routeValues["page"] = value.Page.ToString();
            this.SelfLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;
            
            routeValues["page"] = "1";
            this.FirstLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;

            if (value.Page > 1)
            {
                routeValues["page"] = (value.Page - 1).ToString();
                this.PreviousLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;
            }
            else
            {
                this.PreviousLink = null;
            }

            if (value.Page + 1 <= value.PageCount)
            {
                routeValues["page"] = (value.Page + 1).ToString();
                this.NextLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;
            }
            else
            {
                this.NextLink = null;
            }

            routeValues["page"] = Math.Max(1, value.PageCount).ToString();
            this.LastLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;
        }
    }
}