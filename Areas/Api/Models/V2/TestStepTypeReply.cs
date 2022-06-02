namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestStepTypeReply
    {
        public static TestStepTypeReply From(Shared.Types.TestCase.IStep step)
        {
            return new TestStepTypeReply
            {
                ExchangeType = step.ReplyType.ToApiSpeechDTMFAudioType(),
                Text = step.Reply
            };
        }
    }
}