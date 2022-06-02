namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestResultDetail
    {
        public static TestResultDetail From(VoiceTestResult testcase)
        {
            return new TestResultDetail
            {
                Detail = testcase.DetailedResult,
                Result = testcase.Result.ToApiResultType()
            };
        }

        public static TestResultDetail From(VoiceTestStepResult stepResult)
        {
            return new TestResultDetail
            {
                Detail = stepResult.DetailedResult,
                Result = stepResult.Result.ToApiResultType()
            };
        }

        public static TestResultDetail From(CampaignRunTestResult campaignRunTestResult)
        {
            return new TestResultDetail
            {
                Detail = campaignRunTestResult.DetailedResult,
                Result = campaignRunTestResult.Result.ToApiResultType()
            };
        }

        public static TestResultDetail From(TestStepResultReport testStepResult)
        {
            return new TestResultDetail
            {
                Detail = testStepResult.DetailedResult,
                Result = (ResultType)Enum.Parse(typeof(ResultType), testStepResult.ResultTypeDescription.RemoveWhitespace())
            };
        }
    }
}
