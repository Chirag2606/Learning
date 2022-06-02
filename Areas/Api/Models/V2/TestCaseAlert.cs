namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class TestCaseAlert
    {
        public static TestCaseAlert From(Shared.Types.TestCase.ITestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCaseAlert
            {
                Message = testCase.AlarmMessage ?? string.Empty,
                MajorThresholdCount = testCase.MajorThresholdCriticalCount,
                MinorThresholdCount = testCase.MinorThresholdCriticalCount
            };
        }
    }
}