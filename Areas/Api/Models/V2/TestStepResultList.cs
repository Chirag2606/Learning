namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Shared.Types.Results;

    public partial class TestStepResultList : ITestStepResultList<TestStepResultList>
    {
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
                          CallStepResult = stepResults.Skip(1).Select(V2.CallStepResult.From).ToArray()
                      };

            for (var index = 0; index < ret.CallStepResult.Length; index++)
            {
                ret.CallStepResult[index].Step.StepNo = index + 1;
            }

            return ret;
        }

        // Agent steps not supported in API versions prior to 2.2
        public void UpdateAgentSteps(bool agentStepNotSupported, Guid? ticket)
        {
        }
    }
}