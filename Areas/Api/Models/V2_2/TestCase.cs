namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.TestCase;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Resources;

    public partial class TestCase
    {
        private static readonly FlowMap[] FlowMapList =
            {
                new FlowMap { ApiFlow = CallFlowType.Inbound, ModelFlow = Domain.Types.TestCase.CallFlow.Inbound },
                new FlowMap { ApiFlow = CallFlowType.Outbound, ModelFlow = Domain.Types.TestCase.CallFlow.Outbound },
                new FlowMap { ApiFlow = CallFlowType.Both, ModelFlow = Domain.Types.TestCase.CallFlow.Both }
            };

        public static TestCase From(VoiceTestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCase
                       {
                           Active = true, // What happened to IsActive??
                           Alert = TestCaseAlert.From(testCase),
                           CalledNumber = testCase.PhoneNo,
                           CallFlow = FlowMapList.First(x => x.ModelFlow == testCase.CallFlow).ApiFlow,
                           Description = testCase.Description ?? string.Empty,
                           FolderPath = testCase.FolderPath ?? string.Empty,
                           ModifiedBy = GetModifiedByUsername(testCase.ModifiedBy),
                           Name = testCase.Name,
                           Notes = testCase.Notes ?? string.Empty,
                           PreConnectAudio = testCase.PreConnectAudio ?? string.Empty,
                           TestCaseId = testCase.TestCaseId
                       };
        }

        public static TestCase From(CampaignRunTestResult campaignRunTestResult)
        {
            if (campaignRunTestResult == null)
            {
                return null;
            }

            TestCase result = new TestCase
                                  {
                                      Active = true,
                                      Alert =
                                          new TestCaseAlert
                                              {
                                                  MajorThresholdCount = campaignRunTestResult.MajorThresholdCriticalCount,
                                                  MinorThresholdCount = campaignRunTestResult.MinorThresholdCriticalCount,
                                                  Message = campaignRunTestResult.AlarmMessage,
                                                  Frequency =
                                                      TestSpecificationConversionResultExtensions.Helper.ToAlertFrequencyForModel(
                                                          campaignRunTestResult.AlarmFrequency.ToString())
                                              },
                                      CalledNumber = campaignRunTestResult.PhoneNo,
                                      CallFlow = (CallFlowType)Enum.Parse(typeof(CallFlowType), campaignRunTestResult.CallFlowType.ToString()),
                                      Description = campaignRunTestResult.TestDescription ?? string.Empty,
                                      FolderPath = campaignRunTestResult.FolderPath ?? string.Empty,
                                      Name = campaignRunTestResult.TestCaseName,
                                      Notes = campaignRunTestResult.Notes,
                                      PreConnectAudio = campaignRunTestResult.PreConnectAudio,
                                      TestCaseId = campaignRunTestResult.TestCaseId
                                  };

            result.ModifiedBy = campaignRunTestResult.ModifiedBy.HasValue
                                    ? GetModifiedByUsername(campaignRunTestResult.ModifiedBy)
                                    : GetModifiedByUsername(campaignRunTestResult.TestCaseId, campaignRunTestResult.ActualStartDate);

            return result;
        }

        public static ApiResponse<TestCase> Load(ITestCaseService testCaseService, User user, int account, string scope, int id)
        {
            // Check that the url parameters are correct
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                // Scope must be Voice to get test cases
                return ApiResponse.Fails<TestCase>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl);
            }

            GenericResponse<ITestCase> testCaseResponse = testCaseService.TestCaseGet(AccountRequest.Construct(id, user, account));
            testCaseResponse.ExceptionIfError();

            if (testCaseResponse.Value == null)
            {
                return ApiResponse.NotFoundId<TestCase>(ApiMessages.Entity_TestCase, id);
            }

            return ApiResponse.Succeeds(From(testCaseResponse.Value as VoiceTestCase));
        }

        internal static string GetModifiedByUsername(Guid? guid)
        {
            if (!guid.HasValue)
            {
                return string.Empty;
            }

            IAccountService accountService = DependencyResolver.Current.GetService<IAccountService>();

            GenericResponse<User> userResponse = AsyncHelpers.RunSync(() => accountService.UserGetAsync(new GenericRequest<Guid>(guid.Value)));
            userResponse.ExceptionIfError();

            return userResponse.Value != null ? userResponse.Value.Username : string.Empty;
        }

        private static string GetModifiedByUsername(int testCaseId, DateTime campaignStartDate)
        {
            SessionFacade session = new SessionFacade(HttpContextFactory.Current);

            ITestCaseService testCaseService = DependencyResolver.Current.GetService<ITestCaseService>();

            GenericResponse<ITestCaseHistory> testCaseHistoryResponse = testCaseService.TestCaseHistoryGet(
                new AccountRequest<TestCaseHistoryModifiedRequest>(
                    new TestCaseHistoryModifiedRequest
                        {
                            TestCaseId = testCaseId,
                            ModifiedBefore = campaignStartDate
                        })
                    {
                       AccountId = session.SelectedAccount.Id, User = session.User
                    });

            testCaseHistoryResponse.ExceptionIfError();

            return testCaseHistoryResponse.Value != null ? GetModifiedByUsername(testCaseHistoryResponse.Value.ModifiedBy) : string.Empty;
        }

        private struct FlowMap
        {
            public CallFlowType ApiFlow;

            public CallFlow ModelFlow;
        }
    }
}
