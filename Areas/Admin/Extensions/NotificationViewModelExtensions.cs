namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.CustomerNotification;
    using Cyara.Shared.Types.Notification;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Notification;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Admin.Models.Notification;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Resources;
    using MediatR;
    using Newtonsoft.Json;

    public static class NotificationViewModelExtensions
    {
        private static readonly Type Me = typeof(NotificationViewModelExtensions);

        public static async Task<NotificationViewModel> Prime(this NotificationViewModel value, IMediator mediator, NotificationSettings notificationSettings, bool applyDefaults = true)
        {
            if (applyDefaults)
            {
                value.NewMessage = new NewMessageViewData();
                value.SelectedTab = "new-message";
            }

            value.MaximumAttachmentSizeInBytes = notificationSettings.MaximumAttachmentSizeBytes;
            value.AcceptedAttachmentFileTypes = TransferIntoFileTypeRegex(notificationSettings.AllowedAttachmentExtensions);
            value.NewMessage.TagsJson = SupportedTags.CreateJson();

            // sends async query but does not wait for it yet
            var templatesQuery = new CustomerNotificationsGetTemplatesQuery().Prime(1, MvcApplication.Settings.DefaultPageSize, Columns.Name, true);
            var templatesResponse = mediator.Send(templatesQuery);

            // getting accounts
            var query = new CustomerNotificationsGetAllAccountsQuery();
            var paginatedResponse =await mediator.Send(query);
            
            if (paginatedResponse.Value != null)
            { 
                value.NewMessage.Accounts = paginatedResponse.Value.Select(a => new AccountInfoViewData()
                {
                    AccountId = a.AccountId,
                    Active = a.Active,
                    Name = a.Name,
                    Selected = value.NewMessage.SelectedAccounts != null && value.NewMessage.SelectedAccounts.Contains(a.AccountId),
                    Usernames = string.Join(",", a.Users.Select(b => b.Username))
                });
            }

            // process templates
            var templatesResult = await templatesResponse;
            templatesResult.ExceptionIfError();

            value.Templates = (TemplatesViewData)new TemplatesViewData().FromPaginatedResponseAndQuery(templatesResult, templatesQuery);

            return value;
        }

        public static async Task<NotificationViewModel> SendMessage(this NotificationViewModel value, IMediator mediator, ILogger logger, SessionFacade session, NotificationSettings settings)
        {
            var requestEmailing = CustomerNotificationsSendEmailCommand.Construct(session);
            requestEmailing.Subject = value.NewMessage.Subject;
            requestEmailing.Message = value.NewMessage.MessageBody;
            requestEmailing.AccountIds = value.NewMessage.SelectedAccounts;
            
            try
            {
                var attachments = JsonConvert.DeserializeObject<string[]>(value.NewMessage.Attachments);
                requestEmailing.Attachments = attachments;
            }
            catch (Exception e)
            {
                logger.Exception(Me, "Exception parsing attachments JSON:{0}".FormatWith(value.NewMessage.Attachments), e);

                await value.Prime(mediator, settings, false);
                value.SelectedTab = "new-message";
                value.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(Messages.CustomNotifications_AttachmentsError) };
                return value;
            }
            
            var emailResult = await mediator.Send(requestEmailing);
            emailResult.ExceptionIfError();
            
            if (emailResult.IsSuccess)
            {
                await value.Prime(mediator, settings);
                value.SelectedTab = "email-logs";
                value.Message = new MessageViewData().Prime("EmailMessageSent", Severity.PageSuccess);
            }
            else
            {
                await value.Prime(mediator, settings, false);
                value.SelectedTab = "new-message";
                value.Message = new MessageViewData() { Severity = Severity.PageFatal, Text = new HtmlString(emailResult.ErrorMessage()) };
            }

            return value;
        }

        public static EmailLogsViewData PopulateEmailLogs(this EmailLogsViewData value, IMediator mediator, UrlHelper urlHelper, bool applyDefaults)
        {
            if (applyDefaults || value.PageNumber < 1)
            {
                value.PageNumber = 1;
                value.PageSize = MvcApplication.Settings.DefaultPageSize;
                value.SortAscending = false; // default - latest at the top
                value.SortColumn = Columns.Date;
            }

            var query = new CustomerNotificationsGetEmailLogsQuery().FromPaginatedView(value);
            query.SearchTerm = value.SearchTerm;
            
            var paginatedResponse = AsyncHelpers.RunSync(()=> mediator.Send(query));

            // prepare paginated view from paginated response 
            var paginatedView = new PaginatedView<EmailLogViewData>();
            paginatedView.CollectionSize = paginatedResponse.CollectionSize;
            paginatedView.PageSize = paginatedResponse.PageSize;
            paginatedView.PageNumber = paginatedResponse.CurrentPage;

            var list = new List<EmailLogViewData>();
            foreach (var logEntry in paginatedResponse.Collection)
            {
                var logView = new EmailLogViewData();
                logView.LogId = logEntry.LogId;
                logView.Subject = logEntry.Subject;
                logView.Status = logEntry.SendStatus;
                logView.Date = logEntry.SentDate.ToString(Core.Constants.Common.UtcDateTimeFormatString);

                if (!string.IsNullOrEmpty(logEntry.Attachments))
                {
                    var attachments = JsonConvert.DeserializeObject<string[]>(logEntry.Attachments);
                    logView.Attachments = attachments.Select(
                        a =>
                            {
                                var mediaInfo = MediaInfo.FromString(a);
                                var path = mediaInfo.ToString();
                                var url = urlHelper.Action("Attachment", "Media", new { area = string.Empty, resource = path.Replace(Path.DirectorySeparatorChar, '/') });
                                return new EmailLogViewData.AttachmentItem { FileName = mediaInfo.Name, Url = url };
                            }).ToArray();
                }

                var result = new EmailLogResultViewData();
                if (!string.IsNullOrEmpty(logEntry.ResultDetail))
                {
                    if (logEntry.ResultDetail.StartsWith("{"))
                    {
                        DetailedResult dr = JsonConvert.DeserializeObject<DetailedResult>(logEntry.ResultDetail);
                        if (logEntry.SendStatus == SendStatus.Failed || logEntry.SendStatus == SendStatus.Unknown)
                        {
                            if (dr.Failures != null && dr.Failures.Any())
                            {
                                foreach (var failure in dr.Failures)
                                {
                                    var msg = failure.SmtpError.FailureReason == FailureReason.Smtp
                                                  ? Messages.CustomNotifications_SmtpError.FormatWith(failure.SmtpError.SmtpStatusCode, failure.SmtpError.Detail)
                                                  : failure.SmtpError.Detail;

                                    result.LogFailure(Messages.CustomNotifications_EmailSentFailed, failure.EmailAddresses.ToArray(), msg);
                                }
                            }
                        }
                        else if (logEntry.SendStatus == SendStatus.Sending)
                        {
                            result.LogSending(
                                dr.Remaining + dr.Processed > 0
                                    ? Messages.CustomNotifications_EmailSendingProgress.FormatWith(
                                        Math.Floor(dr.Processed * 100m / (dr.Remaining + dr.Processed)),
                                        dr.Processed,
                                        dr.Remaining + dr.Processed)
                                    : Messages.CustomNotifications_EmailSendingProgress.FormatWith("100", dr.Processed, dr.Remaining + dr.Processed));
                        }
                    }
                    else
                    {
                        result.LogRaw(logEntry.ResultDetail);
                    }
                }

                if (result.LogResults.Count == 0)
                {
                    switch (logEntry.SendStatus)
                    {
                        case SendStatus.Sending:
                            result.LogSending(Messages.CustomNotifications_EmailSendingInProgress);
                            break;
                        case SendStatus.Succeeded:
                            result.LogSucceeded(Messages.CustomNotifications_EmailSentSuccess);
                            break;
                        case SendStatus.Failed:
                            result.LogFailed(Messages.CustomNotifications_EmailSentFailure);
                            break;
                        case SendStatus.Unknown:
                            result.LogFailed(Messages.CustomNotifications_EmailSentUnknown);
                            break;
                    }
                }

                logView.Result = result.ToJson(true).ToJavascriptSafeString();
                list.Add(logView);
            }

            paginatedView.Collection = list;

            value.Logs = paginatedView.Collection;
            value.CollectionSize = paginatedView.CollectionSize;
            value.PageSize = paginatedView.PageSize;
            value.PageNumber = paginatedView.PageNumber;

            return value;
        }

        private static string TransferIntoFileTypeRegex(string commaSeparatedExtensionList)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedExtensionList))
            {
                return string.Empty;
            }

            var extensions = commaSeparatedExtensionList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return @"(\.|\/)({0})".FormatWith(string.Join("|", extensions));
        }
    }
}