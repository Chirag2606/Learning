namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestStepTypeExpect
    {
        public static TestStepTypeExpect From(Shared.Types.TestCase.IStep step)
        {
            return new TestStepTypeExpect
            {
                ExchangeType = step.ExpectType.ToApiSpeechDTMFType(),
                Text = step.Expect
            };
        }
    }
}