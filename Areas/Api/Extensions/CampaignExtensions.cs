namespace Cyara.Web.Portal.Areas.Api.Extensions
{
    using System.Linq;

    using Cyara.Shared.Types.Campaign;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;

    public static class CampaignExtensions
    {
        public static void SetTestCaseProbabilities(this Campaign campaign)
        {
            var maxCurrent = 0;
            if (campaign.TestCases?.Any() ?? false)
            {
                maxCurrent = campaign.TestCases.Max(x => x.OrderNo);
                maxCurrent++;
            }

            // required before save
            CampaignEditViewModel viewModel = new CampaignEditViewModel();
            viewModel.TestCases = Mapper.MapList<Shared.Types.Campaign.CampaignTestCase, TestCaseSelectionViewData>(campaign.TestCases).ToList();
            viewModel.Distribution = campaign.TestCaseDistributionProfile.ToTestCaseDistribution();
            viewModel.SetTestCaseProbability();
            campaign.TestCases = viewModel.TestCases.Select(
                (tc, index) => new Shared.Types.Campaign.CampaignTestCase
                                   {
                                       TestCase = new VoiceTestCase { TestCaseId = tc.TestCaseId },
                                       Probability = (float)(tc.Probability ?? 0),
                                       OrderNo = index + maxCurrent
                                   }).ToList();
        }
    }
}