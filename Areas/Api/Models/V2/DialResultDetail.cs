namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Linq;

    public partial class DialResultDetail
    {
        private static ResultMap[] resultMapList =
            new ResultMap[]
            {
                new ResultMap { ApiResult = DialResultType.Aborted, ModelResult = Domain.Types.TestResult.DialResultType.Aborted },
                new ResultMap { ApiResult = DialResultType.Busy, ModelResult = Domain.Types.TestResult.DialResultType.Busy },
                new ResultMap { ApiResult = DialResultType.Connected, ModelResult = Domain.Types.TestResult.DialResultType.Connected },
                new ResultMap { ApiResult = DialResultType.NoAnswer, ModelResult = Domain.Types.TestResult.DialResultType.NoAnswer },
                new ResultMap { ApiResult = DialResultType.NoResponse, ModelResult = Domain.Types.TestResult.DialResultType.Silence },
                new ResultMap { ApiResult = DialResultType.OperatorMessage, ModelResult = Domain.Types.TestResult.DialResultType.OperatorMessage },
                new ResultMap { ApiResult = DialResultType.OtherFailure, ModelResult = Domain.Types.TestResult.DialResultType.Other },
                new ResultMap { ApiResult = DialResultType.Rejected, ModelResult = Domain.Types.TestResult.DialResultType.Rejected }
            };

        public static DialResultDetail From(Cyara.Shared.Types.Results.VoiceTestResult testcase)
        {
            return new DialResultDetail
            {
                Detail = testcase.DetailedDialResult,
                Result = resultMapList.First(x => x.ModelResult == testcase.DialResult).ApiResult,
            };
        }

        private struct ResultMap
        {
            public DialResultType ApiResult;
            public Domain.Types.TestResult.DialResultType ModelResult;
        }
    }
}