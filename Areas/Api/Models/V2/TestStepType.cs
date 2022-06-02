namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestStepType
    {
        public static TestStepType From(Shared.Types.TestCase.VoiceStep step)
        {
            if (step == null)
            {
                return null;
            }

            return new TestStepType
            {
                BlockPath = string.Empty, // TODO
                Description = step.Description,
                Expect = TestStepTypeExpect.From(step),
                MinConfidenceLevel = step.MinorConfidenceLevel,
                PauseTime = PauseTimeType.From(step),
                PostSpeechSilenceTimeout = (decimal)step.PostSpeechSilenceTimeout,
                Reply = TestStepTypeReply.From(step),
                StepNo = step.StepNo,
                ThresholdTime = ThresholdTimeType.From(step)
            };
        }
    }
}