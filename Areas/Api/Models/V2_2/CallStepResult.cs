namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class CallStepResult
    {
        public static CallStepResult From(VoiceTestStepResult stepResult)
        {
            return new CallStepResult
            {
                Duration = TimeSpan.From(stepResult.Duration),
                ResponseTime = TimeSpan.From(stepResult.Response),
                Step = TestStepType.From(stepResult.Step as VoiceStep),
                StepResult = TestResultDetail.From(stepResult)
            };
        }

        public static CallStepResult From(TestStepResultReport testStepResult)
        {
            return new CallStepResult
            {
                Duration = TimeSpan.FromSeconds(testStepResult.Duration),
                ResponseTime = TimeSpan.FromSeconds(testStepResult.ResponseTime),
                Step = TestStepType.From(testStepResult),
                StepResult = TestResultDetail.From(testStepResult)
            };
        }
    }
}