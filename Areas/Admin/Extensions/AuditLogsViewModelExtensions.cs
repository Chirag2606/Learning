namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Responses;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Core.IO;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Identity.Entity;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Globalization;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Journal;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using MediatR;

    public static class AuditLogsViewModelExtensions
    {
        public static async Task<AuditLogsViewModel> PrimeAsync(this AuditLogsViewModel value, IJournalService journalService, IAccountService accountService, IMediator mediator, User requestingUser, bool applyDefaults = true)
        {
            if (applyDefaults)
            {
                value.From = DateTime.UtcNow.AddDays(-7);
                value.To = DateTime.UtcNow.Date.AddDays(1).AddMinutes(-1);
                value.PageSize = MvcApplication.Settings.DefaultPageSize;
                value.PageNumber = 1;
                value.Action = AuditLogsViewModel.SubmitAction.Update;
                value.SortAscending = false;
                value.SortColumn = Columns.Date;
            }

            value.AdjustFromAndToDates();
            value.DatePattern = DateTimeFormat.GetPickerDateTimeFormat(MvcApplication.Settings);

            var categories = await GetCategoriesAndSubcategories(mediator);
            value.AllCategoriesAsJson = categories.Item2.ToJson();
            value.Categories = categories.Item1.Select(x => new SelectListItem { Value = x, Text = x });

            value.AllUsers = await GetAllUsersAsync(mediator);
            value.AllAccounts = GetAllAccounts(accountService, requestingUser);

            var response =
                journalService.GetJournalRecordsEx(
                    new PaginatedRequest<JournalLookup>(value.ViewModelToJournalLookup())
                    {
                        User = requestingUser,
                        CurrentPage = value.PageNumber,
                        PageSize = value.PageSize,
                        SortField = value.SortColumn,
                        SortAscending = value.SortAscending
                    }, 
                    false);
            response.ExceptionIfError();

            value.FromPaginatedResponse(response);

            return value;
        }

        public static JournalLookup ViewModelToJournalLookup(this AuditLogsViewModel value)
        {
            var criterion = new LookupCriteria();

            if (!string.IsNullOrWhiteSpace(value.Category))
            {
                criterion.Category = value.Category;
            }

            if (!string.IsNullOrWhiteSpace(value.SubCategory))
            {
                criterion.Subcategory = value.SubCategory;
            }

            int accountId;
            if (!string.IsNullOrWhiteSpace(value.AccountName) && int.TryParse(value.AccountName, out accountId))
            {
                criterion.AccountId = accountId;
            }

            Guid userId;
            if (!string.IsNullOrWhiteSpace(value.UserId) && Guid.TryParse(value.UserId, out userId))
            {
                criterion.UserId = userId;
            }
            else if (!string.IsNullOrWhiteSpace(value.UserName) && Guid.TryParse(value.UserName, out userId))
            {
                criterion.UserId = userId;
            }

            value.AdjustFromAndToDates();

            return new JournalLookup { From = value.From, To = value.To, Criteria = new List<LookupCriteria> { criterion } };
        }

        public static async Task<string> ToCsvAsync(this AuditLogsViewModel model, PaginatedResponse<JournalMessageEx> data, IAccountService accountService, User requestingUser, IFileSystem fileSystem, string filePath)
        {
            var builder = new StringBuilder();

            // headers
            var from = model.From.FormatToUtcDateTimeLong();
            var to = model.To.FormatToUtcDateTimeLong();

            builder.AppendFormat("{0}: {1} - {2}{3}", Resources.Common.DateRange, from, to, Environment.NewLine);
            builder.AppendFormat("{0}: {1}{2}", Resources.Common.ExportTimeInUTC, DateTime.UtcNow.FormatToUtcDateTimeLong(), Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(model.Category))
            {
                builder.AppendFormat("{0}: {1}{2}", Resources.Common.TableHeading_Category, model.Category, Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(model.SubCategory))
            {
                builder.AppendFormat("{0}: {1}{2}", Resources.Common.TableHeading_SubCategory, model.SubCategory, Environment.NewLine);
            }

            Guid userId;
            if (!string.IsNullOrWhiteSpace(model.UserId) && Guid.TryParse(model.UserId, out userId))
            {
                builder.AppendFormat("{0}: {1}{2}", Resources.Common.UserId, model.UserId, Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                string username = model.UserName;
                if (Guid.TryParse(model.UserName, out userId))
                {
                    var response = await accountService.UserGetAsync(new GenericRequest<Guid>(userId) { User = requestingUser });
                    if (response.IsSuccess && response.Value != null)
                    {
                        username = response.Value.Username;
                    }
                }

                builder.AppendFormat(
                    "{0}: {1}{2}",
                    Resources.Common.TableHeading_Username,
                    username.ToCsvValue(),
                    Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(model.AccountName))
            {
                var accountName = model.AccountName;
                int accountId;
                if (!string.IsNullOrWhiteSpace(model.AccountName) && int.TryParse(model.AccountName, out accountId))
                {
                    var response = accountService.AccountGet(new GenericRequest<int>(accountId) { User = requestingUser });
                    if (response.IsSuccess && response.Value != null)
                    {
                        accountName = response.Value.Name;
                    }
                }

                builder.AppendFormat("{0}: {1}{2}", Resources.Common.TableHeading_AccountName, accountName.ToCsvValue(), Environment.NewLine);
            }

            builder.AppendFormat("{0}", Environment.NewLine);

            // body
            builder.AppendFormat(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}{11}",
                Resources.Common.TableHeading_DateUTC,
                Resources.Common.TableHeading_UserId,
                Resources.Common.TableHeading_Username,
                Resources.Common.TableHeading_Category,
                Resources.Common.TableHeading_SubCategory,
                Resources.Common.TableHeading_Result,
                Resources.Common.TableHeading_AccountId,
                Resources.Common.TableHeading_AccountName,
                Resources.Common.TableHeading_Session,
                Resources.Common.TableHeading_IPAddress,
                Resources.Common.TableHeading_Details,
                Environment.NewLine);

            fileSystem.AppendAllText(filePath, builder.ToString());
            foreach (var item in data.Collection)
            {
                string[] values =
                    {
                        item.DateCreated?.FormatToUtcDateTimeLong(),
                        item.UserId?.ToString() ?? string.Empty,
                        item.UserName,
                        item.Category,
                        item.SubCategory,
                        item.Result.RemoveNewlines(string.Empty),
                        item.AccountId?.ToString() ?? string.Empty,
                        item.AccountName,
                        item.SessionId,
                        item.IpAddress,
                        item.Detail.RemoveNewlines(string.Empty)
                    };

                fileSystem.AppendAllText(filePath, values.ToCsvLine() + "\r\n");
            }

            return filePath;
        }

        public static async Task<Tuple<IEnumerable<string>, Dictionary<string, List<string>>>> GetCategoriesAndSubcategories(IMediator mediator)
        {
            var response = await mediator.Send(new JournalCategoriesQuery());
            var categories = new List<string> { string.Empty };
            categories.AddRange(response.Keys);
            return new Tuple<IEnumerable<string>, Dictionary<string, List<string>>>(categories, response);
        }

        // Set UTC flag and adjust seconds
        private static void AdjustFromAndToDates(this AuditLogsViewModel value)
        {
            value.From = new DateTime(value.From.Year, value.From.Month, value.From.Day, value.From.Hour, value.From.Minute, 0, DateTimeKind.Utc);
            value.To = new DateTime(value.To.Year, value.To.Month, value.To.Day, value.To.Hour, value.To.Minute, 59, DateTimeKind.Utc);
        }

        private static IEnumerable<SelectListItem> GetAllAccounts(IAccountService accountService, User requestingUser)
        {
            var response = accountService.AccountsGet(
                new PaginatedRequest { CurrentPage = 1, PageSize = int.MaxValue, SortAscending = true, SortField = Columns.AccountName, User = requestingUser });

            if (response.IsSuccess && response.Collection != null)
            {
                var allAccounts = response.Collection.Select(a => new SelectListItem { Text = a.Name, Value = a.AccountId.ToString() }).ToList();
                allAccounts.Insert(0, new SelectListItem { Text = string.Empty, Value = string.Empty });
                return allAccounts;
            }

            return Enumerable.Empty<SelectListItem>();
        }

        private static async Task<IEnumerable<SelectListItem>> GetAllUsersAsync(IMediator mediator)
        {
            var entityUsers = (await mediator.Send(new GetUsersQuery())).OrderBy(y => y.UserName).ToList();
            var allUsers = (from CyaraUserEntity a in entityUsers select new SelectListItem { Text = a.UserName, Value = a.Id.ToString() }).ToList();
            allUsers.Insert(0, new SelectListItem { Text = string.Empty, Value = string.Empty });
            return allUsers;
        }
    }
}
