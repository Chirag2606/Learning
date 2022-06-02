namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestStepTypeReply
    {
        public static TestStepTypeReply From(IStep step)
        {
            return new TestStepTypeReply
                   {
                       ExchangeType = step.ReplyType.ToApiSpeechDTMFAudioType(),
                       Text = step.Reply
                   };
        }

        public static TestStepTypeReply From(TestStepResultReport step)
        {
            return new TestStepTypeReply
                   {
                       ExchangeType = step.ReplyType.ToApiSpeechDTMFAudioType(),
                       Text = step.ReplyWith
                   };
        }
    }
}