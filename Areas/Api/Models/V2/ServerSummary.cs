namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Shared.Types.Agent;

    public partial class ServerSummary
    {
        public static ServerSummary From(Server server)
        {
            if (server == null)
            {
                return null;
            }

            return new ServerSummary
            {
                Name = server.Name,
                ServerId = server.ServerId 
            };
        }
    }
}