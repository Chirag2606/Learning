namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Web.Types.Reports;

    public partial class RingStepResult
    {
        public static RingStepResult From(Shared.Types.Results.VoiceTestStepResult stepResult)
        {
            return new RingStepResult
            {
                Duration = TimeSpan.From(stepResult.Duration),
                ResponseTime = TimeSpan.From(stepResult.Response),
                Step = TestStepRingType.From(stepResult.Step),
                StepResult = TestResultDetail.From(stepResult)
            };
        }

        public static RingStepResult From(TestStepResultReport testStepResult)
        {
            return new RingStepResult
            {
                Duration = TimeSpan.FromSeconds(testStepResult.Duration),
                ResponseTime = TimeSpan.FromSeconds(testStepResult.ResponseTime),
                Step = TestStepRingType.From(testStepResult),
                StepResult = TestResultDetail.From(testStepResult)
            };
        }
    }
}