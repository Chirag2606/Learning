namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;

    public partial class TestCaseSummary
    {
        public static TestCaseSummary From(CampaignRunTestCaseBreakdown breakdown)
        {
            if (breakdown == null)
            {
                return null;
            }

            return new TestCaseSummary
            {
                FolderPath = breakdown.FolderPath ?? string.Empty,
                Name = breakdown.TestCaseName,
                TestCaseId = breakdown.TestCaseId
            };
        }

        public static TestCaseSummary From(Shared.Types.Campaign.CampaignTestCase testCase)
        {
            if (testCase == null || testCase.TestCase == null)
            {
                return null;
            }

            return new TestCaseSummary
            {
                FolderPath = testCase.TestCase.FolderPath ?? string.Empty,
                Name = testCase.TestCase.Name,
                TestCaseId = testCase.TestCase.TestCaseId
            };
        }

        public static TestCaseSummary From(ITestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCaseSummary
            {
                FolderPath = testCase.FolderPath ?? string.Empty,
                Name = testCase.Name,
                TestCaseId = testCase.TestCaseId
            };
        }
    }
}