namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;

    public partial class TestCaseResult2
    {
        public virtual TestCaseResult From(ChatTestCaseHistory history)
        {
            TestCase = TestCase.From(history);
            return this;
        }

        public virtual TestCaseResult From(SmsTestCaseHistory history)
        {
            TestCase = TestCase.From(history);
            return this;
        }

        public virtual TestCaseResult From(EmailTestCaseHistory history)
        {
            TestCase = TestCase.From(history);
            return this;
        }
    }
}