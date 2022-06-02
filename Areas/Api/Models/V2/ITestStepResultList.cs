namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Collections.Generic;

    using Cyara.Shared.Types.Results;

    public interface ITestStepResultList<out TResultList>
    {
        TResultList From(IList<VoiceTestStepResult> stepResults);

        void UpdateAgentSteps(bool agentStepNotSupported, Guid? ticket);
    }
}
