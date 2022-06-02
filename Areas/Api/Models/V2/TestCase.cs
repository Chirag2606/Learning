namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Linq;
    using System.Net;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.TestCase;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Resources;

    public partial class TestCase
    {
        private static FlowMap[] flowMapList = 
            new FlowMap[] 
            { 
                new FlowMap { ApiFlow = CallFlowType.Inbound, ModelFlow = Cyara.Domain.Types.TestCase.CallFlow.Inbound },
                new FlowMap { ApiFlow = CallFlowType.Outbound, ModelFlow = Cyara.Domain.Types.TestCase.CallFlow.Outbound },
                new FlowMap { ApiFlow = CallFlowType.Both, ModelFlow = Cyara.Domain.Types.TestCase.CallFlow.Both } 
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
                CalledNumber = testCase.PhoneNo ?? string.Empty,
                CallFlow = flowMapList.First(x => x.ModelFlow == testCase.CallFlow).ApiFlow,
                Description = testCase.Description ?? string.Empty,
                FolderPath = testCase.FolderPath ?? string.Empty,
                Name = testCase.Name,
                Notes = testCase.Notes ?? string.Empty,
                PreConnectAudio = testCase.PreConnectAudio ?? string.Empty,
                TestCaseId = testCase.TestCaseId
            };
        }

        public static ApiResponse<TestCase> Load(ITestCaseService testCaseService, User user, int account, string scope, int id)
        {
            // Check that the url parameters are correct
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                // Must be Telephone to get test cases
                return ApiResponse.Fails<TestCase>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl);
            }

            var testCaseResponse = testCaseService.TestCaseGet(AccountRequest.Construct(id, user, account));
            testCaseResponse.ExceptionIfError();

            if (testCaseResponse.Value == null)
            {
                return ApiResponse.NotFoundId<TestCase>(ApiMessages.Entity_TestCase, id);
            }

            return ApiResponse.Succeeds(From(testCaseResponse.Value as VoiceTestCase));
        }

        private struct FlowMap
        {
            public CallFlowType ApiFlow;
            public CallFlow ModelFlow;
        }
    }
}