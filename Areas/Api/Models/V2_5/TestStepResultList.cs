namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Internal;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.TestCase;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.TestCase;

    using Newtonsoft.Json;

    public partial class TestStepResultList : ITestStepResultList<TestStepResultList>
    {
        // Not used?
        public static TestStepResultList From(ICollection<TestStepResultReport> stepResults)
        {
            if (stepResults == null || !stepResults.Any())
            {
                return null;
            }

            var ret = new TestStepResultList();
            var stepresult = stepResults.FirstOrDefault(s => s.StepNo == 0);

            if (stepresult == null)
            {
                ret = new TestStepResultList { RingStepResult = null, CallStepResult = stepResults.Select(V2_5.CallStepResult.From).ToArray() };
            }
            else
            {
                ret = new TestStepResultList
                {
                    RingStepResult = RingStepResult.From(stepresult),
                    CallStepResult = stepResults.Skip(1).Select(V2_5.CallStepResult.From).ToArray(),
                    AgentDataStepResult = null
                };
            }

            return ret;
        }

        public TestStepResultList From(IList<VoiceTestStepResult> stepResults)
        {
            var ring = stepResults.FirstOrDefault(s => s.StepNo == 0);
            if (ring == null)
            {
                return null;
            }

            var ret = new TestStepResultList
                      {
                          RingStepResult = RingStepResult.From(ring),
                          CallStepResult = stepResults.Skip(1).Select(V2_5.CallStepResult.From).ToArray(),
                          AgentDataStepResult = null
                      };

            for (var index = 0; index < ret.CallStepResult.Length; index++)
            {
                ret.CallStepResult[index].Step.StepNo = index + 1;
            }

            return ret;
        }

        public TestStepResultList From(IList<ChatTestStepResult> stepResults)
        {
            return FromInternal<ChatTestStepResult>(stepResults.Cast<ITestStepResult>().ToList(), RingStepResult.From, V2_5.CallStepResult.From);
        }

        public TestStepResultList From(IList<SmsTestStepResult> stepResults)
        {
            return FromInternal<SmsTestStepResult>(stepResults.Cast<ITestStepResult>().ToList(), RingStepResult.From, V2_5.CallStepResult.From);
        }

        public TestStepResultList From(IList<EmailTestStepResult> stepResults)
        {
            return FromInternal<EmailTestStepResult>(stepResults.Cast<ITestStepResult>().ToList(), RingStepResult.From, V2_5.CallStepResult.From);
        }

        public void PopulateServiceStepResults(IList<VoiceTestStepResult> stepResults, ITestCaseService testCaseService, int testResultId, MediaType mediaType, int accountId, User user)
        {
            if (stepResults.IsNullOrEmpty())
            {
                return;
            }

            var stepResultResponse = testCaseService.StepResultGet(
                new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(testResultId, mediaType))
                    {
                        AccountId = accountId,
                        User = user
                    });
            stepResultResponse.ExceptionIfError();

            var serviceSteps = stepResultResponse.Value?.Where(x => x.Step?.ExpectType == ExchangeType.ServiceData && !string.IsNullOrEmpty(x.Step?.Expect))
                .Select(x => new { x.StepNo, ServiceStepData = ServiceStepData.From(x.Step.Expect) })
                .ToList();

            foreach (var result in stepResults)
            {
                Domain.Types.TestResult.ServiceStepResult serviceStepResult = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(result.ServiceStepResult))
                    {
                        serviceStepResult = JsonConvert.DeserializeObject<Domain.Types.TestResult.ServiceStepResult>(result.ServiceStepResult);
                    }
                }
                catch (JsonReaderException)
                {
                }

                if (serviceStepResult == null)
                {
                    continue;
                }

                var callStepResult = CallStepResult.SingleOrDefault(x => x.Step.StepNo == result.StepNo);
                if (callStepResult != null && serviceStepResult.TestResultId > 0)
                {
                    callStepResult.ServiceStepResult = new ServiceStepResult { Type = serviceStepResult.MediaType.ToChannel(), TestResultId = serviceStepResult.TestResultId };
                }

                if (serviceSteps == null)
                {
                    continue;
                }

                // attempt to resolve the Channel from the linked test case
                var stepResult = serviceSteps.SingleOrDefault(x => x.StepNo == result.StepNo);
                if (callStepResult != null && stepResult != null)
                {
                    if (callStepResult.ServiceStepResult == null)
                    {
                        var testCaseResponse = testCaseService.TestCaseGet(new AccountRequest<int>(stepResult.ServiceStepData.TestCaseId ?? 0) { AccountId = accountId, User = user });
                        testCaseResponse.ExceptionIfError();

                        if (testCaseResponse?.Value != null)
                        {
                            callStepResult.ServiceStepResult = new ServiceStepResult { Type = testCaseResponse.Value.MediaType.ToChannel(), TestResultId = serviceStepResult.TestResultId };
                        }
                    }

                    var linkedStep = serviceSteps.SingleOrDefault(x => x.StepNo != result.StepNo && x.ServiceStepData.GroupId == stepResult.ServiceStepData.GroupId);
                    if (linkedStep != null && callStepResult.ServiceStepResult != null)
                    {
                        callStepResult.ServiceStepResult.LinkedStepNo = linkedStep.StepNo;
                    }
                }
            }
        }

        private TestStepResultList FromInternal<T>(
            IList<ITestStepResult> stepResults,
            Func<T, RingStepResult> ringStepBuilder,
            Func<T, CallStepResult> callStepBuilder)
        {
            T connect = (T)stepResults.FirstOrDefault(s => s.StepNo == 0);
            if (connect == null)
            {
                return null;
            }

            var ret = new TestStepResultList
                      {
                          RingStepResult = ringStepBuilder(connect),
                          CallStepResult = stepResults.Skip(1).Select(s => callStepBuilder((T)s)).ToArray(),
                          AgentDataStepResult = null
                      };

            for (var index = 0; index < ret.CallStepResult.Length; index++)
            {
                ret.CallStepResult[index].Step.StepNo = index + 1;
            }

            return ret;
        }
    }
}