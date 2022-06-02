namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Web.Portal.Core.Extensions;

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
                       MinorThresholdCount = testCase.MinorThresholdCriticalCount,
                       Frequency =
                           TestSpecificationConversionResultExtensions.Helper.ToAlertFrequencyForModel(testCase.AlarmFrequency.ToString())
                   };
        }
    }
}