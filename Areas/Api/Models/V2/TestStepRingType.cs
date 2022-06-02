namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestStepRingType
    {
        public static TestStepRingType From(Shared.Types.TestCase.IStep step)
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