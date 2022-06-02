namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2;
    using Cyara.Web.Resources;

    using MediatR;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class TestCaseController : BaseApiController
    {
        protected readonly ILogger Logger;

        protected readonly ITestCaseService TestCaseService;

        protected readonly IMediator Mediator;

        public TestCaseController(
            ILogger logger,
            ITestCaseService testCaseService,
            ISchedulerService schedulerService,
            IMediator mediator)
        {
            Logger = logger;
            TestCaseService = testCaseService;
            SchedulerService = schedulerService;
            Mediator = mediator;
            DataHelper = new DataHelper();
        }

        public ISchedulerService SchedulerService { get; }

        /// <summary>
        /// Return a list of test cases defined for the account
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <returns>List of test case summaries</returns>
        [HttpGet, ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.View })]
        public HttpResponseMessage List(int account, string scope)
        {
            var session = new ApiSessionFacade(ControllerContext);
            var loadResponse = TestCaseSummaryList.Load(TestCaseService, session.User, account, scope);
            return loadResponse.Construct(this, Logger);
        }

        /// <summary>
        /// Get the details of a specific test case (id)
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Test Case ID</param>
        [HttpGet, ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.View })]
        public virtual HttpResponseMessage Get(int account, string scope, int id)
        {
            var session = new ApiSessionFacade(ControllerContext);
            var loadResponse = TestCase.Load(TestCaseService, session.User, account, scope, id);
            return loadResponse.Construct(this, Logger);
        }

        /// <summary>
        /// Kick of a testResult immediately in order to test it's validity
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Test case id</param>
        /// <returns>The ticket of ran test case</returns>
        [HttpPut, HttpPost, ActionName("Run")]
        [AuthorizeArea(testCase: new[] { Access.Execute })]
        public HttpResponseMessage TestRun(int account, string scope, int id)
        {
           var session = new ApiSessionFacade(ControllerContext);

            // only voice test cases can be validation at this stage
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Ensure that we can find the test case and that it is valid for this account
            var testCaseResponse = TestCaseService.TestCaseGet(AccountRequest.Construct(id, session.User, account));
            testCaseResponse.ExceptionIfError();
            if (testCaseResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_TestCase, id).Construct(this, Logger);
            }

            // only voice test cases can be validation at this stage
            if (ApiModel.TargetForScope(scope) != testCaseResponse.Value.MediaType)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ScopeMismatch, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Validate the testResult
            if ( AsyncHelpers.RunSync(()=>Mediator.Send(new AccountInMaintenanceQuery(account, testCaseResponse.Value.MediaType, null, true))))
            {
                return ApiResponse.Fails(HttpStatusCode.ServiceUnavailable, ApiMessages.InMaintenanceMode, ApiMessages.Unavailable).Construct(this, Logger);
            }

            var validateRequest = SchedulerService.TestCaseValidate(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(id, testCaseResponse.Value.MediaType)) { AccountId = account, User = session.User });
            validateRequest.ExceptionIfError();
            if (validateRequest.IsSuccess && validateRequest.Value != Guid.Empty)
            {
                return ApiResponse.Succeeds(new TestCaseTicket { Ticket = "{" + validateRequest.Value + "}" }).Construct(this, Logger);
            }

            // We failed for some reason...
            return ApiResponse.Fails(HttpStatusCode.NotAcceptable, Messages.TestCase_ValidateFailed, ApiMessages.Error_UnableToComplete).Construct(this, Logger);
        }

        /// <summary>
        /// Get the status of a test case test-run (no runId)
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Test case id</param>
        /// <param name="entity">Ticket id</param>
        /// <returns>Test case result</returns>
        [HttpGet, ActionName("Run")]
        [AuthorizeArea(testCase: new[] { Access.Execute })]
        public virtual async Task<HttpResponseMessage> TestGet(int account, string scope, int id, Guid? entity = null)
        {
            return await InternalTestGet<TestCaseResult, TestStepResultList>(account, scope, id, true, entity);
        }

        /// <summary>
        /// Get the results of the last/current test case test-run (no runId)
        /// </summary>
        /// <param name="account">Account id</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Test case id</param>
        /// <param name="entity">Ticket id</param>
        /// <returns>Result of the operation</returns>
        [HttpDelete, ActionName("Run")]
        [AuthorizeArea(testCase: new[] { Access.Execute })]
        public HttpResponseMessage TestAbort(int account, string scope, int id, Guid? entity = null)
        {
            // Check that the url parameters are correct
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                // Must be Telephone to get test cases
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            var session = new ApiSessionFacade(ControllerContext);
            if (!entity.HasValue)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Invalid_Url_Entity, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            var response = SchedulerService.TestCaseAbort(new AccountRequest<Guid>(entity.Value)
            {
                AccountId = account,
                User = session.User
            });
            response.ExceptionIfError();

            return ApiResponse.From(response).Construct(this, Logger);
        }

        protected async Task<ApiResponse> TestGetImpl<TTestCase>(Func<VoiceTestResult, VoiceTestCase, IList<VoiceTestStepResult>, TTestCase> factory, int account, string scope, int id, bool enforceLastOnly, Guid? entity = null)
            where TTestCase : class
        {
            // Check that the url parameters are correct
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                // Must be Telephone to get test cases
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl);
            }

            var session = new ApiSessionFacade(ControllerContext);

            // Get the test case details if it exists
            var testCaseResponse = TestCaseService.TestCaseGet(AccountRequest.Construct(id, session.User, account));
            testCaseResponse.ExceptionIfError();
            if (testCaseResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_TestCase, id);
            }

            if (entity == null)
            {
                // for both 2.0 and 2.1 we shall return latest test result
                var resultResponse = TestCaseService.ResultGetByTestCase(AccountRequest.Construct(id, session.User, account));
                resultResponse.ExceptionIfError();

                if (resultResponse.Value != null)
                {
                    // Get step results from DB
                    var stepResult =
                        TestCaseService.StepResultGet(
                            new AccountRequest<Tuple<int, MediaType>>(
                                    Tuple.Create(resultResponse.Value.TestResultId, MediaType.Voice))
                            {
                                AccountId = account,
                                User = session.User
                            });
                    stepResult.ExceptionIfError();
                    return ApiResponse.Succeeds(factory((VoiceTestResult)resultResponse.Value, null, stepResult.Value.Cast<VoiceTestStepResult>().ToList()));
                }

                // there is no last result, we have no ticket specified, return not found
                return new ApiResponse<TestCaseResult>
                                        {
                                            StatusCode = HttpStatusCode.NotFound,
                                            Description = ApiMessages.Content_TestcaseNoValidations,
                                            Reason = ApiMessages.Content_TestcaseNoValidations
                                        };
            }

            Func<ApiResponse> lookupInDb = () =>
            {
                // find this particular ticket
                if (enforceLastOnly == false)
                {
                    var ticketResponse = TestCaseService.ResultGetByTicket(AccountRequest.Construct(entity.Value, session.User, account));
                    ticketResponse.ExceptionIfError();

                    if (ticketResponse.IsSuccess && ticketResponse.Value != null)
                    {
                        if (ticketResponse.Value.TestCaseHistory.TestCaseId == id)
                        {
                            var stepResult =
                                TestCaseService.StepResultGet(
                                    new AccountRequest<Tuple<int, MediaType>>(
                                        Tuple.Create(ticketResponse.Value.TestResultId, MediaType.Voice))
                                    {
                                        AccountId = account,
                                        User = session.User
                                    });
                            stepResult.ExceptionIfError();

                            return ApiResponse.Succeeds(factory((VoiceTestResult)ticketResponse.Value, null, stepResult.Value.Cast<VoiceTestStepResult>().ToList()));
                        }

                        // mismatch between ticket and account
                        return ApiResponse.Fails(HttpStatusCode.Unauthorized, ApiMessages.Unauthorised, ApiMessages.InvalidUrl);
                    }
                }
                else
                {
                    // Find last TestResult
                    var resultResponse = TestCaseService.ResultGetByTestCase(AccountRequest.Construct(id, session.User, account));
                    resultResponse.ExceptionIfError();

                    if (resultResponse.Value != null && resultResponse.Value.TestRunTicket == entity.Value &&
                        resultResponse.Value.TestCaseHistory.TestCaseId == id)
                    {
                        // Get step results from DB
                        var stepResult =
                            TestCaseService.StepResultGet(
                                new AccountRequest<Tuple<int, MediaType>>(
                                        Tuple.Create(resultResponse.Value.TestResultId, MediaType.Voice))
                                {
                                    AccountId = account,
                                    User = session.User
                                });
                        stepResult.ExceptionIfError();
                        return ApiResponse.Succeeds(factory((VoiceTestResult)resultResponse.Value, null, stepResult.Value.Cast<VoiceTestStepResult>().ToList()));
                    }
                }

                return null;
            };

            // Lookup test result in database ticket
            var result = lookupInDb();
            if (result != null)
            {
                // a result or error was found
                return result;
            }

            // No results found in DB, so try get step results from scheduler
            var scheduleStepResponse = await SchedulerService.TestCaseValidationStatus(AccountRequest.Construct(new TestCaseValidationRequest { Ticket = entity.Value, StepNo = 0 }, session.User, account));
            scheduleStepResponse.ExceptionIfError();
            if (scheduleStepResponse.IsSuccess == false || scheduleStepResponse.Value.Finish || scheduleStepResponse.Value.StepResults == null)
            {
                // there is a chance that between checking the db for results the scheduler has finished, so will respond with a null.
                // lets check the db again before returning a 404
                result = lookupInDb();
                if (result != null)
                {
                    // a result or error was found
                    return result;
                }

                return ApiResponse.NotFoundId(ApiMessages.Entity_TestcaseValidation, id);
            }

            // Verify that the steps returned are for the given test case.
            var testStepResults = scheduleStepResponse.Value.StepResults.Cast<VoiceTestStepResult>().ToList();
            var testCaseId = testStepResults.FirstOrDefault()?.TestCaseId;

            if (testCaseId == null || testCaseId == id)
            {
                // There is an edge case here. If the scheduler returns an empty list (no results yet), even if the ticket doesn't belong to the
                // test case, we still return a Success response. While not very harmful, it's logically incorrect.
                return ApiResponse.Succeeds(factory(null, (VoiceTestCase)testCaseResponse.Value, testStepResults));
            }

            // mismatch between Test Case and Ticket
            return ApiResponse.Fails(HttpStatusCode.NotFound, ApiMessages.Content_TestcaseTicketMismatch, string.Empty);
        }

        protected async Task<HttpResponseMessage> InternalTestGet<TResult, TStepResultList>(int account, string scope, int id, bool enforceLastOnly, Guid? entity = null)
            where TResult : class, ITestCaseResult<TResult, TStepResultList>, new()
            where TStepResultList : class, ITestStepResultList<TStepResultList>, new()
        {
            return await InternalTestGet<TResult, TResult, TStepResultList>(account, scope, id, enforceLastOnly, entity);
        }

        protected async Task<HttpResponseMessage> InternalTestGet<TResult, TTestCase, TStepResultList>(int account, string scope, int id, bool enforceLastOnly, Guid? entity = null)
            where TResult : class, TTestCase, ITestCaseResult<TTestCase, TStepResultList>, new()
            where TTestCase : class
            where TStepResultList : class, ITestStepResultList<TStepResultList>, new()
        {
            Func<VoiceTestResult, VoiceTestCase, IList<VoiceTestStepResult>, TResult> factory = (res, tc, step) =>
            {
                TResult ret;

                if (res != null)
                {
                    ret = (TResult)new TResult().From(res, DataHelper);
                }
                else if (tc != null)
                {
                    ret = (TResult)new TResult().From(tc);
                }
                else
                {
                    throw new ArgumentException("res or tc must be specified, both cannot be null");
                }

                ret.TestStepResultList = new TStepResultList().From(step);
                ret.TestStepResultList?.UpdateAgentSteps(true, null);
                return ret;
            };

            ApiResponse output = await TestGetImpl(factory, account, scope, id, enforceLastOnly, entity);
            return output.Construct(this, Logger);
        }
    }
}
