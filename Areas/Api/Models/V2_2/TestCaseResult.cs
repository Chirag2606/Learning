namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class TestCaseResult
    {
        public virtual TestCaseResult From(VoiceTestResult testResult, IDataHelper dataHelper)
        {
            CalledPhoneNumber = testResult.CalledPhoneNo;
            CallingPhoneNumber = testResult.CallingPhoneNo;
            DialResult = DialResultDetail.From(testResult);
            Duration = TimeSpan.From(testResult.Duration);
            RunDate = dataHelper.Output(testResult.ActualStart);
            TestCase = TestCase.From((VoiceTestCase)testResult.TestCaseHistory);
            TestResult = TestResultDetail.From(testResult);
            TestStepResultList = null;
            return this;
        }

        public virtual TestCaseResult From(VoiceTestCase testCase)
        {
            CalledPhoneNumber = string.Empty;
            CallingPhoneNumber = testCase.CallingPhoneNo;
            DialResult = null;
            Duration = null;
            RunDate = null;
            TestCase = TestCase.From(testCase);
            TestResult = new TestResultDetail { Result = ResultType.Success, Detail = "Running" };
            TestStepResultList = null;
            return this;
        }

        public virtual TestCaseResult From(VoiceTestCaseHistory history)
        {
            this.TestCase = TestCase.From(history);
            return this;
        }
    }
}