namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class TestCaseResult2WithCampaignInfo
    {
        public static new TestCaseResult2WithCampaignInfo[] From(IEnumerable<CampaignRunTestResult> testResults, IDataHelper dataHelper)
        {
            if (testResults == null || !testResults.Any())
            {
                return null;
            }

            return testResults.Select(x => new TestCaseResult2WithCampaignInfo().From(x, dataHelper)).Cast<TestCaseResult2WithCampaignInfo>().ToArray();
        }

        public override TestCaseResult From(VoiceTestResult testResult, IDataHelper dataHelper)
        {
            base.From(testResult, dataHelper);

            if (testResult.CampaignRun == null)
            {
                this.Campaign = null;

                return this;
            }

            this.Campaign = new CampaignInfo
                {
                    CampaignId = testResult.CampaignRun.Campaign.CampaignId,
                    CampaignName = testResult.CampaignRun.Campaign.Name,
                    RunId = testResult.RunId
                };

            return this;
        }

        public override TestCaseResult2 From(CampaignRunTestResult campaignRunTestResult, IDataHelper dataHelper)
        {
            base.From(campaignRunTestResult, dataHelper);

            this.Campaign = new CampaignInfo
            {
                CampaignId = campaignRunTestResult.CampaignId,
                CampaignName = campaignRunTestResult.CampaignName,
                RunId = campaignRunTestResult.RunId
            };

            return this;
        }
    }
}