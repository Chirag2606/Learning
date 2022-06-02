namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestStepTypeExpect
    {
        public static TestStepTypeExpect From(IStep step)
        {
            return new TestStepTypeExpect
                   {
                       ExchangeType = step.ExpectType.ToApiSpeechDTMFType(),
                       Text = step.Expect
                   };
        }

        public static TestStepTypeExpect From(TestStepResultReport step)
        {
            return new TestStepTypeExpect
                   {
                       ExchangeType = step.ExpectType.ToApiSpeechDTMFType(),
                       Text = step.ExpectToHear
                   };
        }
    }
}