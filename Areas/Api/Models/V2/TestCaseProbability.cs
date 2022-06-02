namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Extensions;
    using Cyara.Web.Resources;

    public partial class TestCaseProbability
    {
        public static TestCaseProbability From(Shared.Types.Campaign.CampaignTestCase testCase)
        {
            if (testCase == null || testCase.TestCase == null)
            {
                return null;
            }

            return new TestCaseProbability
            {
                FolderPath = testCase.TestCase.FolderPath ?? string.Empty,
                Name = testCase.TestCase.Name,
                TestCaseId = testCase.TestCase.TestCaseId,
                Probability = testCase.Probability
            };
        }
        
        public static TestCaseProbability[] ArrayFrom(IEnumerable<Shared.Types.Campaign.CampaignTestCase> testCases)
        {
            if (testCases == null || testCases.Count() <= 0)
            {
                return null;
            }

            return testCases.Select(x => TestCaseProbability.From(x)).ToArray();
        }

        public static ApiResponse SetOn(TestCaseProbability[] tests, Cyara.Shared.Types.Campaign.Campaign campaign)
        {
            if (tests.Any(x => x.TestCaseId <= 0))
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("TestCase/TestCaseId"), ApiMessages.Content);
            }

            // Ensure that the same testcase hasn't been specified twice
            var dup = tests.GroupBy(x => x.TestCaseId).FirstOrDefault(x => x.Count() > 1);
            if (dup != null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_DuplicateValue.FormatWith("TestCase/TestCaseId", dup.Key), ApiMessages.Content);
            }

            if (campaign.TestCases == null)
            {
                campaign.TestCases = new List<Shared.Types.Campaign.CampaignTestCase>();
            }

            foreach (var test in tests)
            {
                var tc = campaign.TestCases.FirstOrDefault(x => x.TestCase.TestCaseId == test.TestCaseId);

                if (tc != null)
                {
                    campaign.TestCases.Remove(tc);
                }

                campaign.TestCases.Add(new Shared.Types.Campaign.CampaignTestCase
                {
                    TestCase = new VoiceTestCase { TestCaseId = test.TestCaseId },
                    Probability = test.Probability
                });
            }

            campaign.SetTestCaseProbabilities();

            return ApiResponse.Succeeds();
        }
    }
}
