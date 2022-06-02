namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;

    public partial class TestStepType
    {
        public static TestStepType From(VoiceStep step)
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
                           confidenceField = new ConfidenceType { Major = step.MajorConfidenceLevel, Minor = step.MinorConfidenceLevel },
                           PauseTime = PauseTimeType.From(step),
                           PostSpeechSilenceTimeout = (decimal)step.PostSpeechSilenceTimeout,
                           Reply = TestStepTypeReply.From(step),
                           StepNo = step.StepNo,
                           ThresholdTime = ThresholdTimeType.From(step)
                       };
        }

        public static TestStepType From(ChatStep step)
        {
            if (step == null)
            {
                return null;
            }
            
            return new TestStepType
                   {
                       BlockPath = string.Empty,
                       Description = step.Description,
                       Expect = TestStepTypeExpect.From(step),
                       confidenceField = new ConfidenceType { Major = 0F, Minor = 0F },
                       PauseTime = PauseTimeType.From(step),
                       PostSpeechSilenceTimeout = 0M,
                       Reply = TestStepTypeReply.From(step),
                       StepNo = step.StepNo,
                       ThresholdTime = ThresholdTimeType.From(step)
                   };
        }

        public static TestStepType From(SmsStep step)
        {
            if (step == null)
            {
                return null;
            }

            return new TestStepType
                       {
                           BlockPath = string.Empty,
                           Description = step.Description,
                           Expect = TestStepTypeExpect.From(step),
                           confidenceField = new ConfidenceType { Major = 0F, Minor = 0F },
                           PauseTime = PauseTimeType.From(step),
                           PostSpeechSilenceTimeout = 0M,
                           Reply = TestStepTypeReply.From(step),
                           StepNo = step.StepNo,
                           ThresholdTime = ThresholdTimeType.From(step)
                       };
        }

        public static TestStepType From(EmailStep step)
        {
            if (step == null)
            {
                return null;
            }

            return new TestStepType
                   {
                       BlockPath = string.Empty,
                       Description = step.Description,
                       Expect = TestStepTypeExpect.From(step),
                       confidenceField = new ConfidenceType { Major = 0F, Minor = 0F },
                       PauseTime = PauseTimeType.From(step),
                       PostSpeechSilenceTimeout = 0M,
                       Reply = TestStepTypeReply.From(step),
                       StepNo = step.StepNo,
                       ThresholdTime = ThresholdTimeType.From(step)
                   };
        }
    }
}