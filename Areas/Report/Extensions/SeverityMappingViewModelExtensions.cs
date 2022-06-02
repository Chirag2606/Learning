namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.TestResult;
    using Cyara.Shared.Types.Reports;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;

    using Magnum.Extensions;

    using MediatR;

    public static class SeverityMappingViewModelExtensions
    {
        public static async Task<SeverityMappingViewModel> Prime(this SeverityMappingViewModel value, int accountId, IMediator mediator, bool applyDefaults = false)
        {
            var resultCategories = EnumHelper.EnumToList(
                    typeof(TestResultCategory),
                    "TestResultCategory",
                    s => ((TestResultCategory)Enum.Parse(typeof(TestResultCategory), s)).IsHidden());

            if (applyDefaults)
            {
                var customSeverities = (await mediator.Send(new CustomSeverityGetQuery { AccountId = accountId })).ToList();

                var defaultSeverities = (await mediator.Send(new DefaultSeverityGetQuery())).ToList();

                value.DetailedResults = new List<DetailedResultMappingViewData>();

                foreach (var category in resultCategories)
                {
                    var testResultCategory = (TestResultCategory)Enum.Parse(typeof(TestResultCategory), category.Item1);

                    var customSeverity = customSeverities.SingleOrDefault(x => x.TestResultCategory == testResultCategory);

                    CustomSeverityLevel? level = customSeverity?.Severity ?? defaultSeverities.SingleOrDefault(x => x.TestResultCategory == testResultCategory)?.Severity;

                    if (level != null)
                    {
                        value.DetailedResults.Add(
                            new DetailedResultMappingViewData { Name = category.Item2, Severity = level.Value, TestResultCategory = testResultCategory });
                    }
                }
            }
            else
            {
                value.DetailedResults.Each(
                    x => x.Name = resultCategories.Single(c => c.Item1 == x.TestResultCategory.ToString()).Item2);
            }

            value.SeverityChoices =
                EnumHelper.EnumToList(typeof(CustomSeverityLevel), "CustomSeverityLevel", sort: false, ignore: s => s == CustomSeverityLevel.None.ToString())
                    .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                    .ToList();

            return value;
        }
    }
}