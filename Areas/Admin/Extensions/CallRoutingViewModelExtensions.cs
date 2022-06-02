namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Web.Messaging.Types.Command.CallRouting;
    using Cyara.Web.Messaging.Types.Query.CallRouting;
    using Cyara.Web.Portal.Areas.Admin.Models.CallRouting;
    using Cyara.Web.Portal.Core.Extensions;

    using MediatR;

    public static class CallRoutingViewModelExtensions
    {
        private static readonly Type Me = typeof(CallRoutingViewModelExtensions);

        public static async Task<CallRoutingViewModel> Prime(
            this CallRoutingViewModel value,
            IMediator mediator,
            IAccountService accountService,
            SessionFacade session,
            bool applyDefaults)
        {
            if (applyDefaults)
            {
                // this is to retrieve latest ruleset
                var query = new CallRoutingQuery();
                CallRoutingEntity latest = await mediator.Send(query);
                value.RoutingRuleset = latest != null ? latest.RoutingRuleset : string.Empty;

                if (latest != null)
                {
                    value.LastStatus = await ConstructStatusViewData(accountService, latest, session, "LatestSubHeading");
                    value.LastValid = false;
                }
            }

            // this is to retrieve last valid ruleset
            var lastValidQuery = new CallRoutingQuery { LastValid = true };
            CallRoutingEntity lastValid = await mediator.Send(lastValidQuery);

            if (lastValid != null)
            {
                value.LastValidCallRoutingId = lastValid.CallRoutingId;
                value.LastValidCallRoutingRuleset = lastValid.RoutingRuleset;
                value.Hash = lastValid.Sha256;
                value.LastValid = lastValidQuery.LastValid;

                value.LastValidStatus = await ConstructStatusViewData(accountService, lastValid, session, "LastValidSubHeading");
            }

            return value;
        }

        public static async Task<bool> Store(this CallRoutingViewModel value, IMediator mediator, SessionFacade session, ILogger logger)
        {
            try
            {
                var response = await mediator.Send(
                                   new CallRoutingCreateCommand
                                       {
                                           RoutingRuleset = value.RoutingRuleset,
                                           SessionId = session.SessionId,
                                           SystemApproved = value.SystemApproved,
                                           Hash = value.Hash,
                                           LastValid = value.LastValid,
                                           User = session.User
                                       });

                response.ExceptionIfError();

                if (!response.IsSuccess)
                {
                    value.Message = new MessageViewData { Text = new HtmlString(response.ErrorMessage()), Severity = Severity.PageFatal };
                    return false;
                }

                value.CallRoutingId = response.Value.CallRoutingId;
                value.Hash = response.Value.Sha256;

                return true;
            }
            catch (Exception exc)
            {
                logger.Exception(Me, $"Unexpected exception in {nameof(Store)}", exc);
                value.Message = new MessageViewData().Prime("CallRouting_TechnicalError", Severity.PageFatal);
                return false;
            }
        }

        public static async Task<bool> Validate(this CallRoutingViewModel value, IMediator mediator, SessionFacade session, ILogger logger)
        {
            try
            {
                var response = (await mediator.Send(
                                    new CallRoutingValidateCommand
                                        {
                                            CallRoutingId = value.CallRoutingId,
                                            RoutingRuleset = value.RoutingRuleset,
                                            Hash = value.Hash,
                                            SessionId = session.SessionId,
                                            SystemApproved = value.SystemApproved,
                                            User = session.User
                                        })).ToArray();

                response.FirstOrDefault(x => x.Exception != null)?.ExceptionIfError();

                var unsuccessful = response.Where(x => !x.IsSuccess).ToArray();
                if (unsuccessful.Length > 0)
                {
                    var messages = unsuccessful.Select(x => x.ErrorMessage()).Where(x => x != null).OrderBy(x => x).Distinct();
                    value.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(string.Join("<br/>", messages)) };
                }

                var validationIssues = response.SelectMany(x => x.Value?.ValidationMessages ?? Enumerable.Empty<string>()).OrderBy(x => x).Distinct().ToArray();
                value.ValidationIssues = response.Any(x => x.Value?.ValidationMessages != null) ? validationIssues : null;

                return unsuccessful.Length == 0 && validationIssues.Length == 0;
            }
            catch (Exception exc)
            {
                logger.Exception(Me, $"Unexpected exception in {nameof(Validate)}", exc);
                value.Message = new MessageViewData().Prime("CallRouting_TechnicalError", Severity.PageFatal);
                value.ValidationIssues = null;
                return false;
            }
        }

        private static async Task<CallRoutingStatusViewData> ConstructStatusViewData(
            IAccountService accountService,
            CallRoutingEntity entity,
            SessionFacade session,
            string title)
        {
            var userResponse = await accountService.UserGetPlatformUserAsync(new GenericRequest<Guid>(entity.CreatedBy) { User = session.User });
            userResponse.ExceptionIfError();

            var ret = Mapper.Map<CallRoutingEntity, CallRoutingStatusViewData>(entity);
            ret.CreatedBy = userResponse.Value?.Username;
            ret.Title = title;

            return ret;
        }
    }
}