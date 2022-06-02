namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Linq;

    using Cyara.Domain.Types.Common;
    using Cyara.Shared.Types.Agent;
    using Cyara.Shared.Types.TestCase;

    public partial class BehaviourSummary
    {
        private static BehaviourTypeMap[] behaviourTypeMapList 
            = new BehaviourTypeMap[]
            {
                new BehaviourTypeMap
                {
                    ApiType = BehaviourType.Voice,
                    ModelType = MediaType.AgentVoice
                }
            };

        public static BehaviourSummary From(Behaviour behaviour)
        {
            if (behaviour == null)
            {
                return null;
            }

            return new BehaviourSummary
            {
                BehaviourId = behaviour.BehaviourId,
                BehaviourType = behaviourTypeMapList.First(x => x.ModelType == behaviour.MediaTypeId).ApiType,
                Name = behaviour.Name
            };
        }

        private struct BehaviourTypeMap
        {
            public BehaviourType ApiType;
            public MediaType ModelType;
        }
    }
}