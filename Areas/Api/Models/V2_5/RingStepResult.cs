namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Web.Types.Reports;

    public partial class RingStepResult
    {
        public static RingStepResult From(Shared.Types.Results.VoiceTestStepResult stepResult)
        {
            return AutoMapper.Mapper.Map<V2_2.RingStepResult, RingStepResult>(V2_2.RingStepResult.From(stepResult));
        }

        public static RingStepResult From(Shared.Types.Results.ChatTestStepResult stepResult)
        {
            return new RingStepResult
                   {
                       Duration = TimeSpan.From(stepResult.Duration),
                       ResponseTime = TimeSpan.From(stepResult.Response),
                       Step = TestStepRingType.From(stepResult.Step),
                       StepResult = TestResultDetail.From(stepResult)
                   };
        }

        public static RingStepResult From(Shared.Types.Results.SmsTestStepResult stepResult)
        {
            return new RingStepResult
                       {
                           Duration = TimeSpan.From(stepResult.Duration),
                           ResponseTime = TimeSpan.From(stepResult.Response),
                           Step = TestStepRingType.From(stepResult.Step),
                           StepResult = TestResultDetail.From(stepResult)
                       };
        }

        public static RingStepResult From(Shared.Types.Results.EmailTestStepResult stepResult)
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
            return AutoMapper.Mapper.Map<V2_2.RingStepResult, RingStepResult>(V2_2.RingStepResult.From(testStepResult));
        }
    }
}