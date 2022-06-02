namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestResultDetail
    {
        public static TestResultDetail From(Shared.Types.Results.VoiceTestResult testcase)
        {
            return new TestResultDetail
            {
                Detail = testcase.DetailedResult,
                Result = testcase.Result.ToApiResultType()
            };
        }

        public static TestResultDetail From(Shared.Types.Results.VoiceTestStepResult stepResult)
        {
            return new TestResultDetail
            {
                Detail = stepResult.DetailedResult,
                Result = stepResult.Result.ToApiResultType()
            };
        }
    }
}