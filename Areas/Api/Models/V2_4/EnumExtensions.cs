namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumExtensions
    {
        private static readonly TestCaseDistributionMap[] TestCaseDistributionMapList =
        {
            new TestCaseDistributionMap
            {
                Api = TestCaseDistributionProfile.EqualProbability,
                Model = Cyara.Web.Portal.Models.TestCaseDistribution.EqualProbability
            },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.RoundRobin, Model = Cyara.Web.Portal.Models.TestCaseDistribution.RoundRobin },
            new TestCaseDistributionMap
            {
                Api = TestCaseDistributionProfile.UserDefinedProbability,
                Model = Cyara.Web.Portal.Models.TestCaseDistribution.UserDefinedProbability
            },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.SequentialConditional, Model = Cyara.Web.Portal.Models.TestCaseDistribution.SequentialConditional }
        };

        public static Cyara.Web.Portal.Models.TestCaseDistribution ToModelTestCaseDistribution(this TestCaseDistributionProfile value)
        {
            return TestCaseDistributionMapList.First(x => x.Api == value).Model;
        }

        private struct TestCaseDistributionMap
        {
            public TestCaseDistributionProfile Api;
            public Cyara.Web.Portal.Models.TestCaseDistribution Model;
        }
    }
}