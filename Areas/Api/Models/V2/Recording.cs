namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Linq;
    using System.Web;

    using Cyara.Foundation.Core.IO;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.IO;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.TestCase;

    public partial class Recording
    {
        private static readonly Type Me = typeof(Recording);

        public static Recording Prime(HttpContextBase context, int testResultId, int? stepNo, ITestCaseService testCaseService, IFileSystem fileSystem, IMediaAccess mediaAccess, BaseSessionFacade session, ILogger logger)
        {
            string recordingPath = null;

            if (stepNo.HasValue)
            {
                var stepResults = TestRecordingHelper.GetStepResults(session, testResultId, testCaseService);
                var step = stepResults?.SingleOrDefault(x => x.StepNo == stepNo.Value);
                if (step != null)
                {
                    recordingPath = ((VoiceTestStepResult)step).RecordingPath;
                }
                else
                {
                    logger.LogWarnWithFormat(Me, "Unable to locate result for step. TestResultId:{0} AccountId:{1}", testResultId, session.SelectedAccount.Id);
                }
            }
            else
            {
                var recordingInfo = TestRecordingHelper.GetRecordingInfo(session, testResultId, testCaseService);

                if (recordingInfo != null)
                {
                    recordingPath = recordingInfo.RecordingPath;
                }
                else
                {
                    logger.LogWarnWithFormat(Me, "Unable to locate test result. TestResultId:{0} AccountId:{1}", testResultId, session.SelectedAccount.Id);
                }
            }

            if (!string.IsNullOrEmpty(recordingPath))
            {
                var mediaInfo = MediaInfo.FromString(recordingPath);
                var localFilePath = TestRecordingHelper.SaveLocally(logger, context, mediaInfo.Name, testResultId, stepNo, testCaseService, mediaAccess, MvcApplication.Settings);

                if (localFilePath != null)
                {
                    var contentBytes = fileSystem.ReadAllBytes(localFilePath);

                    return new Recording
                               {
                                   ContentType = "audio/wav",
                                   Content = Convert.ToBase64String(contentBytes)
                               };
                }
            }

            logger.LogInfoWithFormat(Me, "No recording found for result. TestResultId:{0} AccountId:{1} StepNo:{2}", testResultId, session.SelectedAccount.Id, stepNo);

            return null;
        }
    }
}