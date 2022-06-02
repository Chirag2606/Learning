namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Domain.Types.TestCase;
    using Cyara.Web.Portal.Models;

    public static class EnumExtensions
    {
        private static readonly ResultMap[] ResultMapList =
        {
            new ResultMap { Api = ResultType.Aborted, Model = Cyara.Domain.Types.TestResult.ResultType.Aborted },
            new ResultMap { Api = ResultType.Failed, Model = Cyara.Domain.Types.TestResult.ResultType.Failed },
            new ResultMap { Api = ResultType.InternalError, Model = Cyara.Domain.Types.TestResult.ResultType.InternalError },
            new ResultMap { Api = ResultType.Satisfactory, Model = Cyara.Domain.Types.TestResult.ResultType.Satisfactory },
            new ResultMap { Api = ResultType.Success, Model = Cyara.Domain.Types.TestResult.ResultType.Success },
            new ResultMap { Api = ResultType.Pending, Model = Cyara.Domain.Types.TestResult.ResultType.Pending }
        };

        private static readonly ReplyActionMap[] ReplyActionMapList =
        {
            new ReplyActionMap { Api  = ReplyActionType.Answer, Model = ExchangeType.Answer },
            new ReplyActionMap { Api  = ReplyActionType.Busy, Model = ExchangeType.Busy },
            new ReplyActionMap { Api  = ReplyActionType.DoNotAnswer, Model = ExchangeType.DoNotAnswer },
            new ReplyActionMap { Api  = ReplyActionType.Reject, Model = ExchangeType.Reject },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.AudioFile },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.DTMF },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Speech }
        };

        private static readonly SpeechDTMFMap[] SpeechDTMFMapList =
        {
            new SpeechDTMFMap { Api = SpeechDTMFType.PESQ, Model = ExchangeType.AudioFile },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.Speech },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.DTMF },
            new SpeechDTMFMap { Api = SpeechDTMFType.AgentData, Model = ExchangeType.AgentData },
            new SpeechDTMFMap { Api = SpeechDTMFType.ServiceStep, Model = ExchangeType.ServiceData }
        };

        private static readonly SpeechDTMFAudioMap[] SpeechDTMFAudioMapList =
        {
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.AudioFile, Model = ExchangeType.AudioFile },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.DTMF, Model = ExchangeType.DTMF },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Speech, Model = ExchangeType.Speech },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.ServiceStep, Model = ExchangeType.ServiceData }
        };

        private static readonly TestCaseDistributionMap[] TestCaseDistributionMapList =
        {
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.EqualProbability, Model = TestCaseDistribution.EqualProbability },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.RoundRobin, Model = TestCaseDistribution.RoundRobin },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.UserDefinedProbability, Model = TestCaseDistribution.UserDefinedProbability },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.SequentialConditional, Model = TestCaseDistribution.SequentialConditional }
        };

        private static readonly Dictionary<TestSpecificationConversionResult.ReportItemType, CyaraXmlImportResultType> Mapping = new Dictionary<TestSpecificationConversionResult.ReportItemType, CyaraXmlImportResultType>
            {
                { TestSpecificationConversionResult.ReportItemType.Error, CyaraXmlImportResultType.Error },
                { TestSpecificationConversionResult.ReportItemType.Information, CyaraXmlImportResultType.Information },
                { TestSpecificationConversionResult.ReportItemType.Progress, CyaraXmlImportResultType.Progress },
                { TestSpecificationConversionResult.ReportItemType.Title, CyaraXmlImportResultType.Title }
            };

        public static ResultType ToApiResultType(this Cyara.Domain.Types.TestResult.ResultType value)
        {
            return ResultMapList.First(x => x.Model == value).Api;
        }

        public static ReplyActionType ToApiReplyActionType(this ExchangeType value)
        {
            return ReplyActionMapList.First(x => x.Model == value).Api;
        }

        public static SpeechDTMFType ToApiSpeechDTMFType(this ExchangeType value)
        {
            return SpeechDTMFMapList.First(x => x.Model == value).Api;
        }

        public static SpeechDTMFAudioType ToApiSpeechDTMFAudioType(this ExchangeType value)
        {
            return SpeechDTMFAudioMapList.First(x => x.Model == value).Api;
        }

        public static Cyara.Web.Portal.Models.TestCaseDistribution ToModelTestCaseDistribution(this TestCaseDistributionProfile value)
        {
            return TestCaseDistributionMapList.First(x => x.Api == value).Model;
        }

        public static CyaraXmlImportResultType? ToCyaraXmlImportResultType(this TestSpecificationConversionResult.ReportItemType value)
        {
            var ret = CyaraXmlImportResultType.Error;
            if (Mapping.TryGetValue(value, out ret))
            {
                return ret;
            }

            return null;
        }

        private struct ResultMap
        {
            public ResultType Api;
            public Cyara.Domain.Types.TestResult.ResultType Model;
        }

        private struct ReplyActionMap
        {
            public ReplyActionType Api;
            public ExchangeType Model;
        }

        private struct SpeechDTMFMap
        {
            public SpeechDTMFType Api;
            public ExchangeType Model;
        }

        private struct SpeechDTMFAudioMap
        {
            public SpeechDTMFAudioType Api;
            public ExchangeType Model;
        }

        private struct TestCaseDistributionMap
        {
            public TestCaseDistributionProfile Api;
            public Cyara.Web.Portal.Models.TestCaseDistribution Model;
        }
    }
}