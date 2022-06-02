namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class CallStepResult
    {
        public static CallStepResult From(Shared.Types.Results.VoiceTestStepResult stepResult)
        {
            return new CallStepResult
            {
                Duration = TimeSpan.From(stepResult.Duration),
                ResponseTime = TimeSpan.From(stepResult.Response),
                Step = TestStepType.From(stepResult.Step as Shared.Types.TestCase.VoiceStep),
                StepResult = TestResultDetail.From(stepResult)
            };
        }
    }
}