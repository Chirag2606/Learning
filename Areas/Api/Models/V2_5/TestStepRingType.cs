namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestStepRingType
    {
        public static TestStepRingType From(IStep step)
        {
            return new TestStepRingType
                   {
                       PauseTime = PauseTimeType.From(step),
                       ReplyType = step.ReplyType.ToApiReplyActionType(),
                       ThresholdTime = ThresholdTimeType.From(step)
                   };
        }

        public static TestStepRingType From(TestStepResultReport step)
        {
            return new TestStepRingType
                   {
                       PauseTime = PauseTimeType.From(step),
                       ReplyType = step.ReplyType.ToApiReplyActionType(),
                       ThresholdTime = ThresholdTimeType.From(step)
                   };
        }
    }
}