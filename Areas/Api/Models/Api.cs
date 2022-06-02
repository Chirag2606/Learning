namespace Cyara.Web.Portal.Areas.Api.Models
{
    using System;
    using System.Linq;
    using System.Net;

    using Cyara.Domain.Types.Common;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Web.Resources;

    public static class ApiModel
    {
        public static MediaType? TargetForScope(string scope, MediaType[] allowed = null)
        {
            Func<MediaType, MediaType?> ensureAllowed = mt =>
                                                        {
                                                            if (allowed == null)
                                                            {
                                                                return mt;
                                                            }

                                                            if (allowed.Contains(mt))
                                                            {
                                                                return mt;
                                                            }

                                                            return null;
                                                        };

            switch (scope.ToLowerInvariant())
            {
                case "agent":
                    return MediaType.AgentVoice;
                case "voice":
                    return MediaType.Voice;
                case "email":
                    return ensureAllowed(MediaType.Email);
                case "web":
                    return ensureAllowed(MediaType.Chat);
                case "sms":
                    return ensureAllowed(MediaType.Sms);
            }

            return null;
        }

        public static CampaignIdentifierWithAbortType CampaignAbortIdentifier(string scope, int id, AbortType abortType)
        {
            var target = TargetForScope(scope);
            if (target.HasValue)
            {
                return new CampaignIdentifierWithAbortType { MediaType = target.Value, CampaignId = id, AbortType = abortType };
            }

            return null;
        }

        public static CampaignIdentifier CampaignIdentifier(string scope, int id)
        {
            var target = TargetForScope(scope);
            if (target.HasValue)
            {
                return new CampaignIdentifier { MediaType = target.Value, CampaignId = id };
            }

            return null;
        }

        public static ApiResponse CheckForErrors(System.Web.Http.ModelBinding.ModelStateDictionary modelState)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var val in modelState.Values.Where(x => x.Errors.Count > 0))
            {
                foreach (var err in val.Errors)
                {
                    sb.AppendLine(err.Exception.Message);
                    var ix = err.Exception.InnerException;
                    while (ix != null)
                    {
                        sb.AppendLine(ix.Message);
                        ix = ix.InnerException;
                    }
                }
            }

            if (sb.Length > 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, sb.ToString(), ApiMessages.Content);
            }

            return null;
        }
    }
}
