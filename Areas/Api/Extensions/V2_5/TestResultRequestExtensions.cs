namespace Cyara.Web.Portal.Areas.Api.Extensions.V2_5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
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
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Resources;

    public static class TestResultRequestExtensions
    {
        private static readonly Type Me = MethodBase.GetCurrentMethod().DeclaringType;

        public static async Task<ApiResponse<TestCaseResult2WithCampaignInfo>> GetTestResults(
            this TestResultRequest value,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            ISchedulerService schedulerService,
            IDataHelper dataHelper)
        {
            var value22 = AutoMapper.Mapper.Map<TestResultRequest, Api.Models.V2_2.TestResultRequest>(value);

            IList<VoiceTestStepResult> stepResults = null;

            var apiResponse = await V2_2.TestResultRequestExtensions.GetTestResults(
                value22,
                sessionId,
                user,
                accountId,
                logger,
                testCaseService,
                schedulerService,
                dataHelper,
                sr =>
                    {
                        stepResults = sr;
                        var results = new Models.V2_5.TestStepResultList().From(sr);
                        return AutoMapper.Mapper.Map<TestStepResultList, Models.V2_2.TestStepResultList>(results);
                    });

            var testCaseResult = AutoMapper.Mapper.Map<Api.Models.V2_2.TestCaseResult2WithCampaignInfo, TestCaseResult2WithCampaignInfo>(apiResponse.Value);

            if (testCaseResult?.TestStepResultList != null)
            {
                testCaseResult.TestStepResultList.PopulateServiceStepResults(stepResults, testCaseService, testCaseResult.TestResultId ?? 0, MediaType.Voice, accountId, user);
            }

            return new ApiResponse<TestCaseResult2WithCampaignInfo>
                {
                    Value = testCaseResult,
                    Description = apiResponse.Description,
                    Exception = apiResponse.Exception,
                    Reason = apiResponse.Reason,
                    StatusCode = apiResponse.StatusCode
                };
        }

        public static ApiResponse<TestCaseResult2WithCampaignInfo> GetChatTestResults(
            this TestResultRequest value,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            IDataHelper dataHelper)
        {
            return value.GetChannelTestResults<ChatTestResult, ChatTestCaseHistory>(
                MediaType.Chat,
                sessionId,
                user,
                accountId,
                logger,
                testCaseService,
                dataHelper,
                (tcr, tr, dh) =>
                {
                    tcr.From(tr, dataHelper);
                },
                (tcr, th) =>
                {
                    tcr.From(th);
                },
                sr => new TestStepResultList().From(sr.Cast<ChatTestStepResult>().ToArray()));
        }

        public static ApiResponse<TestCaseResult2WithCampaignInfo> GetEmailTestResults(
            this TestResultRequest value,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            IDataHelper dataHelper)
        {
            return value.GetChannelTestResults<EmailTestResult, EmailTestCaseHistory>(
                MediaType.Email,
                sessionId,
                user,
                accountId,
                logger,
                testCaseService,
                dataHelper,
                (tcr, tr, dh) =>
                {
                    tcr.From(tr, dataHelper);
                },
                (tcr, th) =>
                {
                    tcr.From(th);
                },
                sr => new TestStepResultList().From(sr.Cast<EmailTestStepResult>().ToArray()));
        }

        public static ApiResponse<TestCaseResult2WithCampaignInfo> GetSmsTestResults(
            this TestResultRequest value,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            IDataHelper dataHelper)
        {
            return value.GetChannelTestResults<SmsTestResult, SmsTestCaseHistory>(
                MediaType.Sms,
                sessionId,
                user,
                accountId,
                logger,
                testCaseService,
                dataHelper,
                (tcr, tr, dh) =>
                    {
                        tcr.From(tr, dataHelper);
                    },
                (tcr, th) =>
                    {
                        tcr.From(th);
                    },
                sr => new TestStepResultList().From(sr.Cast<SmsTestStepResult>().ToArray()));
        }

        private static ApiResponse<TestCaseResult2WithCampaignInfo> GetChannelTestResults<TC, TH>(
            this TestResultRequest value,
            MediaType mediaType,
            string sessionId,
            User user,
            int accountId,
            ILogger logger,
            ITestCaseService testCaseService,
            IDataHelper dataHelper,
            Action<TestCaseResult2WithCampaignInfo, TC, IDataHelper> testCaseBuilder,
            Action<TestCaseResult2WithCampaignInfo, TH> historyBuilder,
            Func<IEnumerable<ITestStepResult>, TestStepResultList> stepResultBuilder)
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

            // this is result as found in database (obtained by either test result id or ticket)
            ITestResult testResultFromDatabase;

            if (value.TestResultId != null)
            {
                if (value.TestResultId.Value < 1)
                {
                    return ApiResponse.NotFoundId<TestCaseResult2WithCampaignInfo>(ApiMessages.Entity_TestResult, value.TestResultId.Value);
                }

                var testResultResponse = testCaseService.ResultGetById(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(value.TestResultId.Value, mediaType))
                {
                    AccountId = accountId,
                    User = user
                });
                testResultResponse.ExceptionIfError();

                if (logger.LogErrorOrEmptyValue(Me, testResultResponse, "SessionId:{0} Unable to get TestResult by TestResultId:{1} Channel:{2}".FormatWith(sessionId, value.TestResultId.Value, mediaType.ToString())))
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
                    logger.LogError(Me, "SessionId: {0} Error executing ResultGetByTicket. Ticket:{1}".FormatWith(sessionId, value.Ticket.Value.ToString()).AddErrorCodeToMessage(testResultResponse.ErrorResult));
                    return ApiResponse<TestCaseResult2WithCampaignInfo>.From(testResultResponse);
                }

                if (testResultResponse.Value == null)
                {
                    // not in the database, return failure
                    return new ApiResponse<TestCaseResult2WithCampaignInfo>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Reason = string.Format(ApiMessages.NotFound, ApiMessages.Entity_TestcaseValidation),
                        Description = string.Format(ApiMessages.NotFound_Id, ApiMessages.Entity_TestcaseValidation, value.Ticket.Value.ToString())
                    };
                }

                // the test result is in the database and we've received it
                testResultFromDatabase = testResultResponse.Value;
            }

            testCaseBuilder(result, (TC)testResultFromDatabase, dataHelper);

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
                    historyBuilder(result, (TH)testCaseHistoryResponse.Value);
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

            result.TestStepResultList = stepResultBuilder(stepResultsResponse.Value);

            return ApiResponse.Succeeds(result);
        }
    }
}
