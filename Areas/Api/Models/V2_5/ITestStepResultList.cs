namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System.Collections.Generic;

    using Cyara.Shared.Types.Results;

    public interface ITestStepResultList<out TResultList>
    {
        TResultList From(IList<VoiceTestStepResult> stepResults);
    }
}