namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
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
            return AutoMapper.Mapper.Map<V2_2.CallStepResult, CallStepResult>(V2_2.CallStepResult.From(testStepResult));
        }

        public static CallStepResult From(ChatTestStepResult stepResult)
        {
            return new CallStepResult
                   {
                       Duration = TimeSpan.From(stepResult.Duration),
                       ResponseTime = TimeSpan.From(stepResult.Response),
                       Step = TestStepType.From(stepResult.Step as ChatStep),
                       StepResult = TestResultDetail.From(stepResult)
                   };
        }

        public static CallStepResult From(SmsTestStepResult stepResult)
        {
            return new CallStepResult
                       {
                           Duration = TimeSpan.From(stepResult.Duration),
                           ResponseTime = TimeSpan.From(stepResult.Response),
                           Step = TestStepType.From(stepResult.Step as SmsStep),
                           StepResult = TestResultDetail.From(stepResult)
                       };
        }

        public static CallStepResult From(EmailTestStepResult stepResult)
        {
            return new CallStepResult
                   {
                       Duration = TimeSpan.From(stepResult.Duration),
                       ResponseTime = TimeSpan.From(stepResult.Response),
                       Step = TestStepType.From(stepResult.Step as EmailStep),
                       StepResult = TestResultDetail.From(stepResult)
                   };
        }
    }
}