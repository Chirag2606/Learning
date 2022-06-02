namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System;
    using System.Linq;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.TestCase;

    public static class EnumExtensions
    {
        private static readonly SpeechDTMFAudioMap[] SpeechDTMFAudioMapList =
        {
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.AudioFile, Model = ExchangeType.AudioFile },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.DTMF, Model = ExchangeType.DTMF },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Speech, Model = ExchangeType.Speech },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Function, Model = ExchangeType.Function },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Message, Model = ExchangeType.Message },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.Notification, Model = ExchangeType.Notification },
            new SpeechDTMFAudioMap { Api = SpeechDTMFAudioType.ServiceStep, Model = ExchangeType.ServiceData }
        };

        private static readonly SpeechDTMFMap[] SpeechDTMFMapList =
        {
            new SpeechDTMFMap { Api = SpeechDTMFType.PESQ, Model = ExchangeType.AudioFile },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.Speech },
            new SpeechDTMFMap { Api = SpeechDTMFType.Speech, Model = ExchangeType.DTMF },
            new SpeechDTMFMap { Api = SpeechDTMFType.Function, Model = ExchangeType.Function },
            new SpeechDTMFMap { Api = SpeechDTMFType.Message, Model = ExchangeType.Message },
            new SpeechDTMFMap { Api = SpeechDTMFType.Notification, Model = ExchangeType.Notification },
            new SpeechDTMFMap { Api = SpeechDTMFType.AgentData, Model = ExchangeType.AgentData },
            new SpeechDTMFMap { Api = SpeechDTMFType.ServiceStep, Model = ExchangeType.ServiceData },
            new SpeechDTMFMap { Api = SpeechDTMFType.Json, Model = ExchangeType.Json }
        };

        private static readonly ReplyActionMap[] ReplyActionMapList =
        {
            new ReplyActionMap { Api  = ReplyActionType.Answer, Model = ExchangeType.Answer },
            new ReplyActionMap { Api  = ReplyActionType.Busy, Model = ExchangeType.Busy },
            new ReplyActionMap { Api  = ReplyActionType.DoNotAnswer, Model = ExchangeType.DoNotAnswer },
            new ReplyActionMap { Api  = ReplyActionType.Reject, Model = ExchangeType.Reject },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.AudioFile },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.DTMF },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Speech },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Message },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Notification },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Function },
            new ReplyActionMap { Api  = ReplyActionType.NONE, Model = ExchangeType.Json }
        };

        public static SpeechDTMFType ToApiSpeechDTMFType(this ExchangeType value)
        {
            return SpeechDTMFMapList.First(x => x.Model == value).Api;
        }

        public static SpeechDTMFAudioType ToApiSpeechDTMFAudioType(this ExchangeType value)
        {
            return SpeechDTMFAudioMapList.First(x => x.Model == value).Api;
        }

        public static ReplyActionType ToApiReplyActionType(this ExchangeType value)
        {
            return ReplyActionMapList.First(x => x.Model == value).Api;
        }

        public static Channel ToChannel(this MediaType value)
        {
            switch (value)
            {
                case MediaType.Chat:
                    return Channel.Web;
                default:
                    return (Channel)Enum.Parse(typeof(Channel), value.ToString(), true);
            }
        }

        private struct ReplyActionMap
        {
            public ReplyActionType Api;
            public ExchangeType Model;
        }

        private struct SpeechDTMFAudioMap
        {
            public SpeechDTMFAudioType Api;
            public ExchangeType Model;
        }

        private struct SpeechDTMFMap
        {
            public SpeechDTMFType Api;
            public ExchangeType Model;
        }
    }
}