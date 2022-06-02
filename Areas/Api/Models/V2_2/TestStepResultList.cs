namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Domain.Types.TestCase;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Portal.Core.Agent;

    public partial class TestStepResultList : V2.ITestStepResultList<TestStepResultList>
    {
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
                ret = new TestStepResultList { RingStepResult = null, CallStepResult = stepResults.Select(V2_2.CallStepResult.From).ToArray() };
            }
            else
            {
                ret = new TestStepResultList
                      {
                          RingStepResult = RingStepResult.From(stepresult),
                          CallStepResult = stepResults.Skip(1).Select(V2_2.CallStepResult.From).ToArray(),
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
                          CallStepResult = stepResults.Skip(1).Select(V2_2.CallStepResult.From).ToArray(),
                          AgentDataStepResult = null
                      };

            for (var index = 0; index < ret.CallStepResult.Length; index++)
            {
                ret.CallStepResult[index].Step.StepNo = index + 1;
            }

            return ret;
        }

        /// <summary>
        /// Replace CallStepResult with Agent data step
        /// </summary>
        public void UpdateAgentSteps(bool agentStepNotSupported, Guid? ticket)
        {
            if (CallStepResult.Any(x => x.Step.Expect.ExchangeType == SpeechDTMFType.AgentData))
            {
                string notSupportedMessage = agentStepNotSupported ? Resources.Common.ResourceManager.GetString("AgentStepNotAvailable") : string.Empty;
                var actual = agentStepNotSupported ? null : AgentStepResultOverride.From(ticket);
                var voiceSteps = CallStepResult.Where(x => x.Step.Expect.ExchangeType != SpeechDTMFType.AgentData);
                var agentSteps = CallStepResult.Where(x => x.Step.Expect.ExchangeType == SpeechDTMFType.AgentData);

                CallStepResult = voiceSteps.ToArray();

                List<AgentDataStepResult> agentStepsList = new List<V2_2.AgentDataStepResult>();
                foreach (var agentStep in agentSteps)
                {
                    agentStepsList.Add(new V2_2.AgentDataStepResult
                    {
                        AgentData = agentStepNotSupported ? 
                                        null : 
                                        AgentVoiceInteraction.From(agentStep.Step.Expect.Text).AgentDataDictionary.Select(
                                                   x => new AgentKeyValuePair
                                                   {
                                                       Key = x.Key,
                                                       Expected = x.Value,
                                                       Actual = actual.GetKeyActualValue(x.Key),
                                                       Result = actual.GetKeyResult(x.Key)?.ToApiResultType() ?? ResultType.InternalError,
                                                   }).ToArray(),
                        Step = new TestStepAgentDataType
                        {
                            StepNo = agentStep.Step.StepNo,
                            Description = agentStep.Step.Description
                        },
                        StepResult = new TestResultDetail
                        {
                            Result = agentStepNotSupported ? ResultType.Failed : actual.Result.ToApiResultType(),
                            Detail = agentStepNotSupported ? notSupportedMessage : actual.Error
                        }
                    });
                }

                AgentDataStepResult = agentStepsList.ToArray();
            }
        }
    }
}