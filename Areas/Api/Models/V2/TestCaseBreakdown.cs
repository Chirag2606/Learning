namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Shared.Web.Types.Reports;

    public partial class TestCaseBreakdown
    {
        public static TestCaseBreakdown From(CampaignRunTestCaseBreakdown breakdown)
        {
            return new TestCaseBreakdown
            {
                Calls = breakdown.Calls,
                Percentage = (float)breakdown.Percentage,
                TestCase = TestCaseSummary.From(breakdown)
            };
        }

        public static TestCaseBreakdown[] ArrayFrom(ICollection<CampaignRunTestCaseBreakdown> breakdown)
        {
            if (breakdown == null || breakdown.Count() <= 0)
            {
                return null;
            }

            return breakdown.Select(x => TestCaseBreakdown.From(x)).ToArray();
        }
    }
}