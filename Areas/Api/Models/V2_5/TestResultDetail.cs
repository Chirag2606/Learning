namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;

    public partial class TestResultDetail
    {
        public static TestResultDetail From(VoiceTestStepResult stepResult)
        {
            return AutoMapper.Mapper.Map<V2_2.TestResultDetail, TestResultDetail>(V2_2.TestResultDetail.From(stepResult));
        }
        
        public static TestResultDetail From(TestStepResultReport testStepResult)
        {
            return AutoMapper.Mapper.Map<V2_2.TestResultDetail, TestResultDetail>(V2_2.TestResultDetail.From(testStepResult));
        }

        public static TestResultDetail From(ChatTestResult testcase)
        {
            return new TestResultDetail
                   {
                       Detail = testcase.DetailedResult,
                       Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(testcase.Result.ToApiResultType())
                   };
        }

        public static TestResultDetail From(SmsTestResult testcase)
        {
            return new TestResultDetail
                       {
                           Detail = testcase.DetailedResult,
                           Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(testcase.Result.ToApiResultType())
                       };
        }

        public static TestResultDetail From(EmailTestResult testcase)
        {
            return new TestResultDetail
                   {
                       Detail = testcase.DetailedResult,
                       Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(testcase.Result.ToApiResultType())
                   };
        }

        public static TestResultDetail From(ChatTestStepResult stepResult)
        {
            return new TestResultDetail
                   {
                       Detail = stepResult.DetailedResult,
                       Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(stepResult.Result.ToApiResultType())
                   };
        }

        public static TestResultDetail From(SmsTestStepResult stepResult)
        {
            return new TestResultDetail
                       {
                           Detail = stepResult.DetailedResult,
                           Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(stepResult.Result.ToApiResultType())
                       };
        }

        public static TestResultDetail From(EmailTestStepResult stepResult)
        {
            return new TestResultDetail
                   {
                       Detail = stepResult.DetailedResult,
                       Result = AutoMapper.Mapper.Map<V2_2.ResultType, ResultType>(stepResult.Result.ToApiResultType())
                   };
        }
    }
}