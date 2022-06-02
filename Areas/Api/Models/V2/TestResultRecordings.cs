namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Linq;
    using System.Web;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Core.IO;

    public partial class TestResultRecordings
    {
        private static readonly Type Me = typeof(TestResultRecordings);

        public static TestResultRecordings Prime(HttpContextBase context, int testResultId, ITestCaseService testCaseService, BaseSessionFacade session, ILogger logger)
        {
            var recordings = new TestResultRecordings();

            var testResult = testCaseService.ResultGetById(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(testResultId, MediaType.Voice))
            {
                AccountId = session.SelectedAccount.Id,
                User = session.User
            });
            testResult.ExceptionIfError();

            if (testResult.Value == null)
            {
                logger.LogWarnWithFormat(Me, "Unable to locate test result. TestResultId:{0} AccountId:{1}", testResultId, session.SelectedAccount.Id);

                return null;
            }

            Func<string, bool> fileExists = f => !string.IsNullOrEmpty(f) && !string.IsNullOrEmpty(FileUtility.GetAudioFileNameIfExists(f));

            recordings.TestRecording = fileExists(((VoiceTestResult)testResult.Value).RecordingPath);

            var stepResults = testCaseService.StepResultGet(new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(testResultId, MediaType.Voice))
            {
                AccountId = session.SelectedAccount.Id,
                User = session.User
            });
            stepResults.ExceptionIfError();

            if (stepResults.Value == null)
            {
                logger.LogWarnWithFormat(Me, "Unable to locate result for step. TestResultId:{0} AccountId:{1}", testResultId, session.SelectedAccount.Id);

                return null;
            }

            recordings.Steps = stepResults.Value
                .OrderBy(x => x.StepNo)
                .Select(x => new StepRecordings
                {
                    StepNo = x.StepNo,
                    StepRecording = fileExists(((VoiceTestStepResult)x).RecordingPath)
                }).ToArray();

            return recordings;
        }
    }
}