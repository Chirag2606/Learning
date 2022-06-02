namespace Cyara.Web.Portal.Areas.Api.Extensions.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Resources;

    public static class TestResultRequestExtensions
    {
        private static readonly Type Me = typeof(TestResultRequestExtensions);

        public static async Task<ApiResponse<TestCaseResult2WithCampaignInfo>> GetTestResults(
            this TestResultRequest value,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            ISchedulerService schedulerService,
            IDataHelper dataHelper,
            Func<IList<VoiceTestStepResult>, TestStepResultList> stepResultGenerator = null)
        {
            // validate the parameters - cannot use both
            if (value.TestResultId != null && value.Ticket != null)
            {
                return ApiResponse<TestCaseResult2WithCampaignInfo>.ValidationFails(ApiMessages.TestResultIdTicketNotBoth);
            }

            // must use at least one
            if (value.TestResultId == null && value.Ticket == null)
            {
                return ApiResponse<TestCaseResult2WithCampaignInfo>.ValidationFails(ApiMessages.TestResultIdTicketParameterRequired);
            }

            // return value
            var result = new TestCaseResult2WithCampaignInfo();

            // this is result as found in database (obtained by either testresultid or ticket)
            ITestResult testResultFromDatabase = null;

            if (value.TestResultId != null)
            {
                if (value.TestResultId.Value < 1)
                {
                    return ApiResponse.NotFoundId<TestCaseResult2WithCampaignInfo>(ApiMessages.Entity_TestResult, value.TestResultId.Value);
                }

                var testResultResponse = testCaseService.ResultGetById(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(value.TestResultId.Value, MediaType.Voice))
                {
                    AccountId = accountId,
                    User = user
                });
                testResultResponse.ExceptionIfError();

                if (logger.LogErrorOrEmptyValue(Me, testResultResponse, "SessionId:{0} Unable to get TestResult by TestResultId:{1}".FormatWith(sessionId, value.TestResultId.Value)))
                {
                    if (testResultResponse.IsSuccess)
                    {
                        // successful, so we have found nothing
                        return ApiResponse.NotFoundId<TestCaseResult2WithCampaignInfo>(ApiMessages.Entity_TestResult, value.TestResultId.Value);
                    }

                    // error
                    return ApiResponse<TestCaseResult2WithCampaignInfo>.From(testResultResponse);
                }

                // at this point we have a test result in DB, test step results will be retrieved later
                testResultFromDatabase = testResultResponse.Value;
            }
            else
            {
                // get the test result by the ticket here
                var testResultResponse = testCaseService.ResultGetByTicket(new AccountRequest<Guid>(value.Ticket.Value)
                {
                    AccountId = accountId,
                    User = user
                });
                testResultResponse.ExceptionIfError();

                if (testResultResponse.IsSuccess == false)
                {
                    logger.LogError(Me, $"SessionId: {sessionId} Error executing ResultGetByTicket. Ticket:{value.Ticket.Value}".AddErrorCodeToMessage(testResultResponse.ErrorResult));
                    return ApiResponse<TestCaseResult2WithCampaignInfo>.From(testResultResponse);
                }

                // if not in the database yet, try the scheduler instead
                if (testResultResponse.Value == null)
                {
                    var request = new TestCaseValidationRequest { Ticket = value.Ticket.Value, StepNo = 0 };
                    var testCaseSchedulerResult = await schedulerService.TestCaseValidationStatus(
                        new AccountRequest<TestCaseValidationRequest>(request) { User = user, AccountId = accountId });
                    testCaseSchedulerResult.ExceptionIfError();

                    if (testCaseSchedulerResult.IsSuccess == false)
                    {
                        logger.LogError(
                            Me,
                            $"SessionId: {sessionId} Error executing TestCaseValidationStatus. Ticket:{value.Ticket.Value}".AddErrorCodeToMessage(testCaseSchedulerResult.ErrorResult));
                        return ApiResponse<TestCaseResult2WithCampaignInfo>.From(testCaseSchedulerResult);
                    }

                    if (testCaseSchedulerResult.Value.Finish == false && testCaseSchedulerResult.Value.StepResults != null)
                    {
                        // test case is still executing and known by the scheduler, return what we have so far
                        result.TestStepResultList =
                            new TestStepResultList().From(testCaseSchedulerResult.Value.StepResults.Cast<VoiceTestStepResult>().ToList());
                        result.TestStepResultList?.UpdateAgentSteps(false, value.Ticket);
                        return ApiResponse.Succeeds(result);
                    }

                    // not in the database, not in the scheduler, return failure
                    return new ApiResponse<TestCaseResult2WithCampaignInfo>
                               {
                                   StatusCode = HttpStatusCode.NotFound,
                                   Reason = string.Format(
                                       ApiMessages.NotFound,
                                       ApiMessages.Entity_TestcaseValidation),
                                   Description = string.Format(
                                       ApiMessages.NotFound_Id,
                                       ApiMessages.Entity_TestcaseValidation,
                                       value.Ticket.Value.ToString())
                               };
                }

                // the test result is in the database and we've received it
                testResultFromDatabase = testResultResponse.Value;
            }

            result.From((VoiceTestResult)testResultFromDatabase, dataHelper);

            if (testResultFromDatabase.ActualStart != null)
            {
                var testCaseHistoryResponse = testCaseService.TestCaseHistoryGet(new AccountRequest<TestCaseHistoryModifiedRequest>(
                                new TestCaseHistoryModifiedRequest
                                {
                                    TestCaseId = testResultFromDatabase.TestCaseHistory.TestCaseId,
                                    ModifiedBefore = testResultFromDatabase.ActualStart.Value
                                })
                {
                    AccountId = accountId,
                    User = user
                });
                testCaseHistoryResponse.ExceptionIfError();

                // if no luck here
                if (!testCaseHistoryResponse.IsSuccess)
                {
                    logger.LogDebugWithFormat(Me, "SessionId:{0} Failed to find test case history via TestCaseHistoryGet for TestCaseId:{1} ModifiedBefore:{2}", sessionId, testResultFromDatabase.TestCaseHistory.TestCaseId, testResultFromDatabase.ActualStart.Value);
                    return ApiResponse<TestCaseResult2WithCampaignInfo>.From(testCaseHistoryResponse);
                }

                if (testCaseHistoryResponse.Value != null)
                {
                    // map this test case to the result
                    result.From((VoiceTestCaseHistory)testCaseHistoryResponse.Value);
                }
            }

            // collect all the step results here
            var stepResultsResponse = testCaseService.StepResultGet(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(testResultFromDatabase.TestResultId, testResultFromDatabase.MediaType))
            {
                AccountId = accountId,
                User = user
            });
            stepResultsResponse.ExceptionIfError();

            if (logger.LogErrorOrEmptyValue(Me, stepResultsResponse, "SessionId:{0} Failed to get step results for TestResultId:{1}".FormatWith(sessionId, testResultFromDatabase.TestResultId)))
            {
                return ApiResponse<TestCaseResult2WithCampaignInfo>.From(stepResultsResponse);
            }

            result.TestStepResultList = stepResultGenerator == null ? new TestStepResultList().From(stepResultsResponse.Value.Cast<VoiceTestStepResult>().ToArray()) : stepResultGenerator(stepResultsResponse.Value.Cast<VoiceTestStepResult>().ToArray());

            result.TestStepResultList?.UpdateAgentSteps(false, testResultFromDatabase.TestRunTicket);

            return ApiResponse.Succeeds(result);
        }
    }
}
