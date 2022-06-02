namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class TestCaseResult2WithCampaignInfo
    {
        public TestCaseResult From(ChatTestResult testResult, IDataHelper dataHelper)
        {
            Url = testResult.CallingUrl;
            Duration = TimeSpan.From(testResult.Duration);
            RunDate = dataHelper.Output(testResult.ActualStart);
            TestCase = TestCase.From((ChatTestCase)testResult.TestCaseHistory);
            TestResult = TestResultDetail.From(testResult);
            TestStepResultList = null;
            Type = Channel.Web;
            TestResultId = testResult.TestResultId;

            if (testResult.CampaignRun == null)
            {
                Campaign = null;
                return this;
            }

            Campaign = new CampaignInfo
                       {
                           CampaignId = testResult.CampaignRun.Campaign.CampaignId,
                           CampaignName = testResult.CampaignRun.Campaign.Name,
                           RunId = testResult.RunId
                       };

            return this;
        }

        public TestCaseResult From(EmailTestResult testResult, IDataHelper dataHelper)
        {
            EmailTo = testResult.EmailTo;
            Duration = TimeSpan.From(testResult.Duration);
            RunDate = dataHelper.Output(testResult.ActualStart);
            TestCase = TestCase.From((EmailTestCase)testResult.TestCaseHistory);
            TestResult = TestResultDetail.From(testResult);
            TestStepResultList = null;
            Type = Channel.Email;
            TestResultId = testResult.TestResultId;

            if (testResult.CampaignRun == null)
            {
                Campaign = null;
                return this;
            }

            Campaign = new CampaignInfo
                       {
                           CampaignId = testResult.CampaignRun.Campaign.CampaignId,
                           CampaignName = testResult.CampaignRun.Campaign.Name,
                           RunId = testResult.RunId
                       };

            return this;
        }

        public TestCaseResult From(SmsTestResult testResult, IDataHelper dataHelper)
        {
            Mobile = testResult.Mobile;
            Duration = TimeSpan.From(testResult.Duration);
            RunDate = dataHelper.Output(testResult.ActualStart);
            TestCase = TestCase.From((SmsTestCase)testResult.TestCaseHistory);
            TestResult = TestResultDetail.From(testResult);
            TestStepResultList = null;
            Type = Channel.Sms;
            TestResultId = testResult.TestResultId;

            if (testResult.CampaignRun == null)
            {
                Campaign = null;
                return this;
            }

            Campaign = new CampaignInfo
                           {
                               CampaignId = testResult.CampaignRun.Campaign.CampaignId,
                               CampaignName = testResult.CampaignRun.Campaign.Name,
                               RunId = testResult.RunId
                           };

            return this;
        }
    }
}