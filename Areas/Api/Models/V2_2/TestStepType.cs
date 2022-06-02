namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestStepType
    {
        public static TestStepType From(VoiceStep step)
        {
            if (step == null)
            {
                return null;
            }

            //// var bstep = step as Shared.Types.TestCase.BlockTestStep;
            return new TestStepType
            {
                BlockPath = string.Empty, // TODO
                Description = step.Description,
                Expect = TestStepTypeExpect.From(step),
                confidenceField  = ConfidenceType.From(step),
                PauseTime = PauseTimeType.From(step),
                PostSpeechSilenceTimeout = (decimal)step.PostSpeechSilenceTimeout,
                Reply = TestStepTypeReply.From(step),
                StepNo = step.StepNo,
                ThresholdTime = ThresholdTimeType.From(step)
            };
        }

        public static TestStepType From(TestStepResultReport step)
        {
            if (step == null)
            {
                return null;
            }

            return new TestStepType
            {
                StepNo = step.StepNo,
                Description = step.Description,
                Expect = TestStepTypeExpect.From(step),
                BlockPath = string.Empty, // TODO
                confidenceField = ConfidenceType.From(step),
                PauseTime = PauseTimeType.From(step),
                PostSpeechSilenceTimeout = (decimal)step.PostSpeechSilenceTimeout,
                Reply = TestStepTypeReply.From(step),
                ThresholdTime = ThresholdTimeType.From(step)
            };
        }
    }
}