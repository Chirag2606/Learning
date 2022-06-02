namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Core;

    public interface ITestCaseResult<out TResult, TResultList> where TResultList : ITestStepResultList<TResultList>
    {
        TResultList TestStepResultList { get; set; }

        TResult From(VoiceTestResult testResult, IDataHelper dataHelper);

        TResult From(VoiceTestCase testCase);
    }
}
