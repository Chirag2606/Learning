namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Cyara.Domain.Types.Common;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Resources;

    public partial class TestCaseSummaryList
    {
        public static TestCaseSummaryList From(IEnumerable<ITestCase> testCases)
        {
            if (testCases == null || testCases.Count() <= 0)
            {
                return null;
            }

            return new TestCaseSummaryList { TestCase = testCases.Select(x => TestCaseSummary.From(x)).ToArray() };
        }

        public static ApiResponse<TestCaseSummaryList> Load(ITestCaseService testCaseService, User user, int account, string scope)
        {
            // Check that the url parameters are correct
            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                // Must be Telephone to get test cases
                return ApiResponse.Fails<TestCaseSummaryList>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl);
            }

            var testCaseResponse = testCaseService.TestCaseByAccount(AccountRequest.Construct(MediaType.Voice, user, account));
            testCaseResponse.ExceptionIfError();

            return ApiResponse.Succeeds(From(testCaseResponse.Value.OrderBy(x => x.TestCaseId)));
        }
    }
}