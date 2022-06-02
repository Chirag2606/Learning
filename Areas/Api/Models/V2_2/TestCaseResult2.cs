namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Areas.Api.Models.V2;

    public partial class TestCaseResult2 : ITestCaseResult<TestCaseResult, TestStepResultList>
    {
        public static TestCaseResult2[] From(IEnumerable<CampaignRunTestResult> testResults, IDataHelper dataHelper)
        {
            if (testResults == null || !testResults.Any())
            {
                return null;
            }

            return testResults.Select(x => new TestCaseResult2().From(x, dataHelper)).ToArray();
        }

        public override TestCaseResult From(VoiceTestResult testResult, IDataHelper dataHelper)
        {
            base.From(testResult, dataHelper);
            TestResultId = testResult.TestResultId;
            return this;
        }

        public virtual TestCaseResult2 From(CampaignRunTestResult campaignRunTestResult, IDataHelper dataHelper)
        {
            CalledPhoneNumber = campaignRunTestResult.CalledPhoneNumber;
            CallingPhoneNumber = campaignRunTestResult.CallingPhoneNumber;
            DialResult = DialResultDetail.From((Domain.Types.TestResult.DialResultType)campaignRunTestResult.DialResultId, campaignRunTestResult.DetailedDialResult);
            Duration = TimeSpan.FromSeconds((float)campaignRunTestResult.Duration);
            RunDate = dataHelper.Output(campaignRunTestResult.ActualStartDate);
            TestCase = TestCase.From(campaignRunTestResult);
            TestResult = TestResultDetail.From(campaignRunTestResult);
            TestStepResultList = null;
            TestResultId = campaignRunTestResult.TestResultId;

            return this;
        }
    }
}