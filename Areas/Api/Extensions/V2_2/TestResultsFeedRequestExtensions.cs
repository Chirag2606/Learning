namespace Cyara.Web.Portal.Areas.Api.Extensions.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Settings;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Resources;

    using PlanType = Cyara.Domain.Types.Plan.PlanType;

    public static class TestResultsFeedRequestExtensions
    {
        private static readonly Type Me = typeof(TestResultsFeedRequestExtensions);

        private static readonly string[] AcceptableDateTimeFormats = new[]
        {
            "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFZ",
            "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFzzz"
        };

        public static ApiResponse<TestResultFeed> GetTestResultsFeedWithAccount(
            this TestResultsFeedRequest value,
            NameValueCollection query,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            IReportsService reportService,
            IAccountService accountService,
            IDataHelper dataHelper,
            WebSiteSettings settings,
            ApiController context)
        {
            if (value.PlanType == null)
            {
                return ApiResponse<TestResultFeed>.ValidationFails(ApiMessages.PlanTypeRequired);
            }

            if (value.FromDate == null && value.FromTestResultId == null)
            {
                return ApiResponse<TestResultFeed>.ValidationFails(ApiMessages.FromDateOrTestResultParameterRequired);
            }

            if (value.FromDate != null && value.FromTestResultId != null)
            {
                return ApiResponse<TestResultFeed>.ValidationFails(ApiMessages.FromDateOrTestResultNotBoth);
            }

            // do all the validation checks and extract parameters to use
            int maxResults = MvcApplication.Settings.ApiDefaultFeedChunkSize;
            bool includeStepResults = value.StepResults ?? false;
            DateTime? fromDate = null;
            DateTime parseResult;

            if (value.FromDate != null)
            {
                if (!DateTime.TryParseExact(value.FromDate, AcceptableDateTimeFormats, null, DateTimeStyles.RoundtripKind, out parseResult))
                {
                    return ApiResponse<TestResultFeed>.ValidationFails("{0}: {1}".FormatWith("FromDate", ApiMessages.InvalidDateFormat));
                }

                fromDate = parseResult.ToUniversalTime();
            }

            if (value.MaxResults != null)
            {
                if (value.MaxResults.Value < 1 || value.MaxResults.Value > MvcApplication.Settings.ApiMaximumFeedChunkSize)
                {
                    return ApiResponse<TestResultFeed>.ValidationFails(
                        "{0}: {1}".FormatWith("MaxResults", ApiMessages.MaxResultsBetween.FormatWith(MvcApplication.Settings.ApiMaximumFeedChunkSize)));
                }

                maxResults = value.MaxResults.Value;
            }

            // validation is done, grab the result and run with it
            var response = ExtractFeedData(
                sessionId,
                value.PlanType.Value,
                MediaType.Voice,
                value.FromTestResultId,
                fromDate,
                user,
                accountId,
                logger,
                reportService,
                accountService,
                dataHelper,
                maxResults,
                includeStepResults,
                query,
                context);

            return response;
        }

        private static ApiResponse<TestResultFeed> ExtractFeedData(
            string sessionId,
            PlanType planType,
            MediaType mediaType,
            int? fromTestResultId,
            DateTime? fromDate,
            User user,
            int accountId,
            ILogger logger,
            IReportsService reportService,
            IAccountService accountService,
            IDataHelper dataHelper,
            int maxResults,
            bool includeStepResults,
            NameValueCollection queryString,
            ApiController context)
        {
            var testRunTickets = new Dictionary<int, Guid?> { { -1, null } };

            // go to the service
            var reportResponse = reportService.TestResultsFeedGet(new AccountRequest<TestResultsFeed>(
                new TestResultsFeed
                {
                    FromTestResultId = fromTestResultId,
                    FromDate = fromDate,
                    PlanType = planType,
                    FeedChunkSize = maxResults,
                    MediaType = mediaType
                })
                {
                    User = user,
                    AccountId = accountId
                });
            reportResponse.ExceptionIfError();

            if (!reportResponse.IsSuccess)
            {
                logger.LogError(Me, "SessionId:{0} Unable to retrieve test results feed. ErrorCode: {1}".FormatWith(sessionId, reportResponse.ErrorResult));
                return ApiResponse<TestResultFeed>.From(reportResponse);
            }

            List<TestCaseResult2WithCampaignInfo> feedItems;
            if (reportResponse.Value != null && reportResponse.Value.TestResults != null)
            {
                feedItems = new List<TestCaseResult2WithCampaignInfo>();

                var resp = TestCaseResult2WithCampaignInfo.From(reportResponse.Value.TestResults, dataHelper);

                if (resp != null)
                {
                    feedItems.AddRange(resp);

                    if (includeStepResults)
                    {
                        foreach (var tr in reportResponse.Value.TestResults)
                        {
                            testRunTickets[tr.TestResultId] = tr.TestRunTicket;
                        }
                    }
                }
            }
            else
            {
                feedItems = new List<TestCaseResult2WithCampaignInfo>();
            }

            // if we are to return steps, populate them now
            if (feedItems.Count > 0 && includeStepResults)
            {
                var testStepResultsResponse =
                    reportService.TestStepResultsGetByTestResultIds(
                        new AccountRequest<Tuple<IEnumerable<int>, MediaType>>(
                            Tuple.Create(feedItems.Select(fi => fi.TestResultId ?? 0).Distinct(), mediaType))
                        {
                            User = user, AccountId = accountId
                        });

                testStepResultsResponse.ExceptionIfError();

                if (!testStepResultsResponse.IsSuccess)
                {
                    logger.LogError(Me, "SessionId:{0} Unable to retrieve test step results".FormatWith(sessionId).AddErrorCodeToMessage(testStepResultsResponse.ErrorResult, false));

                    return ApiResponse<TestResultFeed>.From(testStepResultsResponse);
                }

                if (testStepResultsResponse.Value != null)
                {
                    var materialised = testStepResultsResponse.Value.ToList();

                    feedItems.AsParallel().ForEach(
                        fi =>
                            {
                                fi.TestStepResultList = TestStepResultList.From(materialised.Where(item => item.TestResultId == fi.TestResultId).ToArray());
                                fi.TestStepResultList?.UpdateAgentSteps(false, testRunTickets[fi.TestResultId ?? -1]);
                            });
                }
            }

            // get the account in question
            var accountResponse = accountService.AccountGet(new GenericRequest<int>(accountId) { User = user });
            accountResponse.ExceptionIfError();

            if (logger.LogErrorOrEmptyValue(Me, accountResponse, "SessionId:{0} Unable to retrieve account for AccountId:{1}".FormatWith(sessionId, accountId)))
            {
                return ApiResponse<TestResultFeed>.From(accountResponse);
            }

            // output value
            var returnVal = new TestResultFeed
            {
                AccountId = accountId,
                AccountName = accountResponse.Value.Name,
                Data = feedItems.ToArray(),
                MatchedResults = reportResponse.Value.TotalRecords,
                ReturnedResults = feedItems.Count,
                StepResults = includeStepResults.ToString()
            };

            GenerateFeedLinks(returnVal, context, queryString, reportResponse.Value.FromResultId);

            // return what we have gathered
            return ApiResponse.Succeeds(returnVal);
        }

        private static void GenerateFeedLinks(TestResultFeed result, ApiController context, NameValueCollection query, int? marker)
        {
            const string RouteName = "API_V2_0_Report_Filtered";

            var routeValues = query.AllKeys.ToDictionary(k => k.ToLower(), k => (object)query[k]);

            routeValues.Add("controller", "run");

            result.SelfLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;

            if (marker.HasValue)
            {
                routeValues["fromtestresultid"] = marker.Value.ToString();
                routeValues.Remove("fromdate");
            }

            result.NextLink = new Uri(context.Request.RequestUri, context.Url.Route(RouteName, routeValues)).AbsoluteUri;
        }
    }
}
