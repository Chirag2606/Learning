namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Domain.Types.TestCase;
    using Cyara.Web.Portal.Models;

    public static class EnumExtensions
    {
        private static readonly ResultMap[] ResultMapList = new ResultMap[]
        {
            new ResultMap { Api = ResultType.Aborted, Model = Cyara.Domain.Types.TestResult.ResultType.Aborted },
            new ResultMap { Api = ResultType.Failed, Model = Cyara.Domain.Types.TestResult.ResultType.Failed },
            new ResultMap { Api = ResultType.InternalError, Model = Cyara.Domain.Types.TestResult.ResultType.InternalError },
            new ResultMap { Api = ResultType.Satisfactory, Model = Cyara.Domain.Types.TestResult.ResultType.Satisfactory },
            new ResultMap { Api = ResultType.Success, Model = Cyara.Domain.Types.TestResult.ResultType.Success }
        };

        // Reply field in Ring step
        private static readonly ReplyActionMap[] ReplyActionMapList = new ReplyActionMap[]
        {
            new ReplyActionMap { Api  = ReplyActionType.Answer, Model = ExchangeType.Answer },
            new ReplyActionMap { Api  = ReplyActionType.Busy, Model = ExchangeType.Busy },
            new ReplyActionMap { Api  = ReplyActionType.DoNotAnswer, Model = ExchangeType.DoNotAnswer },
            new ReplyActionMap { Api  = ReplyActionType.Reject, Model = ExchangeType.Reject },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.AudioFile },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.DTMF },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Speech }
        };

        // expect field
        private static readonly SpeechDTMFMap[] SpeechDTMFMapList = new SpeechDTMFMap[]
        {
            new SpeechDTMFMap { Api = SpeechDTMFType.PESQ, Model = ExchangeType.AudioFile },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.Speech },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.DTMF },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.AgentData }, // not supported in ver 2
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.ServiceData }
        };

        // Reply field in all steps other then Ring step
        private static readonly SpeechDTMFAudioMap[] SpeechDTMFAudioMapList = new SpeechDTMFAudioMap[]
        {
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.AudioFile, Model = ExchangeType.AudioFile },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.DTMF, Model = ExchangeType.DTMF },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Speech, Model = ExchangeType.Speech }
        };

        private static readonly TestCaseDistributionMap[] TestCaseDistributionMapList = new TestCaseDistributionMap[]
        {
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.EqualProbability, Model = Cyara.Web.Portal.Models.TestCaseDistribution.EqualProbability },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.RoundRobin, Model = Cyara.Web.Portal.Models.TestCaseDistribution.RoundRobin },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.UserDefinedProbability, Model = Cyara.Web.Portal.Models.TestCaseDistribution.UserDefinedProbability },
            new TestCaseDistributionMap { Api = TestCaseDistributionProfile.SequentialConditional, Model = Cyara.Web.Portal.Models.TestCaseDistribution.SequentialConditional }
        };

        private static readonly Dictionary<Portal.Models.TestSpecificationConversionResult.ReportItemType, CyaraXmlImportResultType> Mapping = new Dictionary<TestSpecificationConversionResult.ReportItemType, CyaraXmlImportResultType>
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

        public static CyaraXmlImportResultType? ToCyaraXmlImportResultType(this Cyara.Web.Portal.Models.TestSpecificationConversionResult.ReportItemType value)
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