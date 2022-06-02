namespace Cyara.Web.Portal.Areas.Api.Extensions.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Resources;

    using CampaignRun = Cyara.Web.Portal.Areas.Api.Models.V2.CampaignRun;
    using CampaignRunRequest = Cyara.Web.Portal.Areas.Api.Models.V2_2.CampaignRunRequest;

    public static class CampaignRunRequestExtensions
    {
        private static readonly Type Me = typeof(CampaignRunRequestExtensions);

        public static ApiResponse<CampaignRunResults> GetCampaignRunTestResults(this CampaignRunRequest value, string sessionId, User user, int accountId, ILogger logger, ICampaignService campaignService, ISchedulerService schedulerService, IReportsService reportService, IAccountService accountService, IDataHelper dataHelper)
        {
            var preparationResult = value.PrepareCampaignRunValues(sessionId, user, accountId, logger, campaignService, schedulerService, reportService, accountService, dataHelper);
            if (preparationResult.StatusCode != HttpStatusCode.OK)
            {
                return ApiResponse<CampaignRunResults>.Fails(preparationResult);
            }

            var result = new CampaignRunResults
            {
                CampaignRun2 = preparationResult.Value
            };

            if (result.CampaignRun2.RunId == 0)
            {
                // there are no runs to check, so just return the instance above, with empty status
                result.CampaignRun2.CampaignStatus = CampaignStatus.None;
                return ApiResponse.Succeeds(result);
            }

            // this will be setup by parallel execution below in case of error
            ApiResponse<CampaignRunResults> finalResult = null;

            var runDetails = campaignService.CampaignRunGetIncludeRealTimeStatus(
                new AccountRequest<int>(result.CampaignRun2.RunId)
                {
                    AccountId = accountId,
                    User = user
                });
            runDetails.ExceptionIfError();

            if (!runDetails.IsSuccess)
            {
                logger.LogErrorWithFormat(Me, "Error while retrieving campaign real time status. SessionId:{0} ErrorCode:{1} ErrorMessage:{2}", sessionId, runDetails.Code(), runDetails.ErrorMessage());
                finalResult = ApiResponse<CampaignRunResults>.From(runDetails);
            }
            else
            {
                // real time status as determined by the service call
                result.CampaignRun2.CampaignStatus = (CampaignStatus)Enum.Parse(typeof(CampaignStatus), runDetails.Value.Item2);
                if (result.CampaignRun2.CampaignStatus == CampaignStatus.Running)
                {
                    result.CampaignRun2.Status = CampaignRunStatus.Running;
                }
            }

            PaginatedResponse<CampaignRunTestResult> testResults = reportService.CampaignRunTestResultsByRunIdGet(
                       new PaginatedRequest<int>(preparationResult.Value.RunId)
                       {
                           User = user,
                           AccountId = accountId,
                           PageSize = int.MaxValue,
                           CurrentPage = 1
                       });

            testResults.ExceptionIfError();

            if (!testResults.IsSuccess)
            {
                // failed retrieving results
                logger.LogError(Me, "Failed retrieving campaign run test results. ErrorCode:{0} ErrorMessage:{1}".FormatWith(testResults.Code(), testResults.ErrorMessage()));
                finalResult = ApiResponse<CampaignRunResults>.From(testResults);
            }
            else
            {
                result.TestCaseResults = TestCaseResult2.From(testResults.Collection, dataHelper);
            }

            return finalResult ?? ApiResponse.Succeeds(result);
        }

        public static ApiResponse<CampaignRunStepResults> GetCampaignRunTestStepResults(this CampaignRunRequest value, NameValueCollection query, string sessionId, User user, int accountId, ILogger logger, ICampaignService campaignService, ISchedulerService schedulerService, IReportsService reportService, ITestCaseService testCaseService, ApiController context, MediaType mediaType, IAccountService accountService, IDataHelper dataHelper)
        {
            var testRunTickets = new Dictionary<int, Guid?> { { -1, null } };

            var preparationResult = value.PrepareCampaignRunValues(sessionId, user, accountId, logger, campaignService, schedulerService, reportService, accountService, dataHelper);
            if (preparationResult.StatusCode != HttpStatusCode.OK)
            {
                return ApiResponse<CampaignRunStepResults>.Fails(preparationResult);
            }

            var result = new CampaignRunStepResults
            {
                CampaignRun2 = preparationResult.Value
            };

            if (preparationResult.Value.RunId == 0)
            {
                // if we did not find any runs, return empty result
                var pagination = CampaignRunStepResults.Prime(query, 0);
                result.GeneratePaginatedLinks(pagination, context);
                return ApiResponse.Succeeds(result);
            }

            // remove the campaign id and replace with the run id.  this will ensure pagination uses a consistent key
            query.Remove("CampaignId");
            query.Remove("RunId");
            query.Add("RunId", preparationResult.Value.RunId.ToString());
            value.RunId = preparationResult.Value.RunId;

            var paginatedResult = CampaignRunStepResults.Prime(query);

            // get the page worth of the results first
            PaginatedResponse<CampaignRunTestResult> testResults = reportService.CampaignRunTestResultsByRunIdGet(
                        new PaginatedRequest<int>(preparationResult.Value.RunId)
                        {
                            User = user,
                            AccountId = accountId,
                            PageSize = paginatedResult.PerPage,
                            CurrentPage = paginatedResult.Page
                        });
            testResults.ExceptionIfError();

            if (!testResults.IsSuccess)
            {
                // failed retrieving results
                logger.LogError(Me, "Failed retrieving campaign run test results. ErrorCode:{0} ErrorMessage:{1}".FormatWith(testResults.Code(), testResults.ErrorMessage()));
                return ApiResponse<CampaignRunStepResults>.From(testResults);
            }

            paginatedResult.Prime(query, testResults.CollectionSize);
            if (testResults.CollectionSize > 0)
            {
                foreach (var tr in testResults.Collection)
                {
                    testRunTickets[tr.TestResultId] = tr.TestRunTicket;
                }
                
                paginatedResult.Data = TestCaseResult2.From(testResults.Collection, dataHelper).ToList();
            }

            // this will be setup by parallel execution below in case of error
            ApiResponse<CampaignRunStepResults> finalResult = null;

            // now we have the page worth of IDs, we'll do parallel invocation for campaign run details and step results
            var runDetails = campaignService.CampaignRunGetIncludeRealTimeStatus(
                new AccountRequest<int>(preparationResult.Value.RunId)
                {
                    AccountId = accountId,
                    User = user
                });
            runDetails.ExceptionIfError();

            if (!runDetails.IsSuccess)
            {
                logger.LogErrorWithFormat(Me, "Error while retrieving campaign real time status. SessionId:{0} ErrorCode:{1} ErrorMessage:{2}", sessionId, runDetails.Code(), runDetails.ErrorMessage());
                finalResult = ApiResponse<CampaignRunStepResults>.From(runDetails);
            }
            else
            {
                // real time status as determined by the service call
                result.CampaignRun2.CampaignStatus = (CampaignStatus)Enum.Parse(typeof(CampaignStatus), runDetails.Value.Item2);
                if (result.CampaignRun2.CampaignStatus == CampaignStatus.Running)
                {
                    result.CampaignRun2.Status = CampaignRunStatus.Running;
                }
            }

            if (paginatedResult.Data != null && paginatedResult.Data.Count > 0)
            {
                var testStepResults = reportService.TestStepResultsGetByTestResultIds(
                    new AccountRequest<Tuple<IEnumerable<int>, MediaType>>(
                        Tuple.Create(paginatedResult.Data.Select(x => x.TestResultId ?? 0), mediaType))
                    {
                        User = user,
                        AccountId = accountId
                    });
                testStepResults.ExceptionIfError();

                if (!testStepResults.IsSuccess)
                {
                    // failed retrieving results
                    logger.LogError(Me, "Failed retrieving campaign run test step results. ErrorCode:{0} ErrorMessage:{1}".FormatWith(testStepResults.Code(), testStepResults.ErrorMessage()));
                    finalResult = ApiResponse<CampaignRunStepResults>.From(testStepResults);
                }
                else
                {
                    var materialisedList = testStepResults.Value.ToArray();

                    paginatedResult.Data.AsParallel().ForEach(
                        tr =>
                            {
                                var list = TestStepResultList.From(materialisedList.Where(x => x.TestResultId == tr.TestResultId).ToArray());
                                list?.UpdateAgentSteps(false, testRunTickets[tr.TestResultId ?? -1]);
                                tr.TestStepResultList = list;
                            });

                    result.TestCaseResults = paginatedResult.Data.ToArray();
                }
            }

            result.GeneratePaginatedLinks(paginatedResult, context);

            return finalResult ?? ApiResponse.Succeeds(result);
        }

        private static ApiResponse<CampaignRun2> PrepareCampaignRunValues(this CampaignRunRequest value, string sessionId, User user, int accountId, ILogger logger, ICampaignService campaignService, ISchedulerService schedulerService, IReportsService reportService, IAccountService accountService, IDataHelper dataHelper)
        {
            // return value
            var result = new CampaignRun2();

            if ((value.RunId ?? 0) == 0 && (value.CampaignId ?? 0) == 0)
            {
                return ApiResponse<CampaignRun2>.ValidationFails(ApiMessages.RunIdCampaignIdParameterRequired);
            }

            if ((value.RunId ?? 0) > 0 && (value.CampaignId ?? 0) > 0)
            {
                return ApiResponse<CampaignRun2>.ValidationFails(ApiMessages.RunIdCampaignIdNotBoth);
            }

            if (value.CampaignId != null)
            {
                var response = campaignService.CampaignGet(new AccountRequest<CampaignIdentifier>(
                    new CampaignIdentifier
                    {
                        CampaignId = value.CampaignId.Value,
                        MediaType = MediaType.Voice // CCM Campaigns not supported currently
                    })
                {
                    AccountId = accountId,
                    User = user
                });
                response.ExceptionIfError();

                if (logger.LogErrorOrEmptyValue(Me, response, "CampaignGet IsSuccess is false. SessionId:{0} ErrorCode:{1} ErrorMessage:{2}".FormatWith(sessionId, response.Code(), response.ErrorMessage())))
                {
                    return ApiResponse.NotFoundId<CampaignRun2>(ApiMessages.Entity_Campaign, value.CampaignId.Value);
                }

                // at this moment, we have a campaign here
                result.CampaignId = response.Value.CampaignId;

                result.CampaignName = response.Value.Name;

                // determine the last run id
                var latestRunResponse = campaignService.CampaignRunsGetLatestRunId(
                    new AccountRequest<int>(value.CampaignId.Value)
                    {
                        AccountId = accountId,
                        User = user
                    });
                latestRunResponse.ExceptionIfError();

                if (latestRunResponse.Value != null)
                {
                    // result field
                    result.RunId = latestRunResponse.Value.Value;

                    var campaignRunSummary = campaignService.CampaignRunGet(AccountRequest.Construct(result.RunId, user, accountId));
                    campaignRunSummary.ExceptionIfError();

                    if (!campaignRunSummary.IsSuccess || campaignRunSummary.Value == null)
                    {
                        return ApiResponse.NotFoundId<CampaignRun2>(ApiMessages.Entity_CampaignRun, value.RunId.Value);
                    }

                    result.Status = (CampaignRunStatus)CampaignRun.StatusFrom(campaignRunSummary.Value.Result);
                    result.StartDate = dataHelper.Output(campaignRunSummary.Value.StartDate);
                    result.EndDate = dataHelper.Output(campaignRunSummary.Value.EndDate);
                }
                else
                {
                    result.Status = CampaignRunStatus.Success;
                    result.StartDate = dataHelper.Output(DateTime.MinValue);
                    result.EndDate = null;
                }
            }
            else
            {
                if (value.RunId == null)
                {
                    throw new Exception("RunId is null in PrepareCampaignRunValues() - Validation Failure");
                }

                // result field - still needs campaign
                result.RunId = value.RunId.Value;

                var campaignRunSummary = campaignService.CampaignRunGet(AccountRequest.Construct(result.RunId, user, accountId));
                campaignRunSummary.ExceptionIfError();

                if (!campaignRunSummary.IsSuccess || campaignRunSummary.Value == null)
                {
                    return ApiResponse.NotFoundId<CampaignRun2>(ApiMessages.Entity_CampaignRun, value.RunId.Value);
                }

                // Ensure the run is for the right area...
                var planResponse = accountService.PlanGet(AccountRequest.Construct(campaignRunSummary.Value.Plan.PlanId, user, accountId));
                planResponse.ExceptionIfError();

                if (!planResponse.IsSuccess || planResponse.Value == null || planResponse.Value.MediaType != MediaType.Voice)
                {
                    return ApiResponse.NotFoundId<CampaignRun2>(ApiMessages.Entity_CampaignRun, value.CampaignId ?? value.RunId ?? 0);
                }

                result.CampaignId = campaignRunSummary.Value.Campaign.CampaignId;
                result.CampaignName = campaignRunSummary.Value.Campaign.Name;

                result.Status = (CampaignRunStatus)CampaignRun.StatusFrom(campaignRunSummary.Value.Result);
                result.StartDate = dataHelper.Output(campaignRunSummary.Value.StartDate);
                result.EndDate = dataHelper.Output(campaignRunSummary.Value.EndDate);
            }

            return ApiResponse.Succeeds(result);
        }
    }
}
