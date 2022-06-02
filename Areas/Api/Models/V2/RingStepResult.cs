namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
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
    }
}