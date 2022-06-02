namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Portal.Areas.Api.Core;

    using Newtonsoft.Json;

    /// <summary>
    /// I do not think this class is used anywhere, it is just a copy of V2\CampaignRunSummaryResults.cs
    /// </summary>
    public partial class CampaignRunSummaryResults
    {
        /// <summary>
        /// Using field "[field name]Specified" we control visibility of related fields "[field name]", 
        /// <see cref="http://stackoverflow.com/questions/5818513/xml-serialization-hide-null-values"/>
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public bool EndDateSpecified { get; set; }

        public static CampaignRunSummaryResults From(Cyara.Shared.Web.Types.Scheduler.CCMRunningCampaign run, List<Cyara.Shared.Web.Types.Scheduler.CCMAgentStatus> agents, IDataHelper dataHelper)
        {
            if (run == null || agents == null)
            {
                return null;
            }

            return new CampaignRunSummaryResults
                {
                    Name = run.Name,
                    RunId = run.RunId,
                    StartDate = dataHelper.Output(run.StartDate),
                    EndDate = null,
                    AgentStatusList = AgentStatus.ArrayFrom(agents)
                };
        }

        public static CampaignRunSummaryResults From(Cyara.Shared.Types.Campaign.CampaignRun run, ICollection<CampaignRunTestCaseBreakdown> breakdown, IEnumerable<Tuple<Cyara.Domain.Types.TestResult.ResultType, int>> results, IDataHelper dataHelper)
        {
            if (breakdown == null || results == null)
            {
                return null;
            }

            return new CampaignRunSummaryResults
            {
                Name = run.Campaign != null ? run.Campaign.Name : string.Empty,
                RunId = run.RunId,
                StartDate = dataHelper.Output(run.StartDate),
                EndDate = dataHelper.Output(run.EndDate),
                ResultSummaryList = ResultSummary.ArrayFrom(results),
                TestCaseBreakdownList = TestCaseBreakdown.ArrayFrom(breakdown)
            };
        }
    }
}