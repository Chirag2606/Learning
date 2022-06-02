namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class ResultSummary
    {
        public static ResultSummary From(Tuple<Cyara.Domain.Types.TestResult.ResultType, int> result)
        {
            return new ResultSummary
            {
                Result = result.Item1.ToApiResultType(),
                Number = result.Item2
            };
        }

        public static ResultSummary[] ArrayFrom(IEnumerable<Tuple<Cyara.Domain.Types.TestResult.ResultType, int>> results)
        {
            if (results == null || results.Count() <= 0)
            {
                return null;
            }

            return results.Select(x => ResultSummary.From(x)).ToArray();
        }
    }
}