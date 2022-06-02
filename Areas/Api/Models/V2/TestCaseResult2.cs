namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class TestCaseResult2
    {
        public override TestCaseResult From(Shared.Types.Results.VoiceTestResult testResult, IDataHelper dataHelper)
        {
            base.From(testResult, dataHelper);
            TestResultId = testResult.TestResultId;
            return this;
        }
    }
}