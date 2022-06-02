// <copyright file="TestCaseController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace

namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;

    using MediatR;

    public class TestCaseController : V2_4.TestCaseController
    {
        public TestCaseController(ILogger logger, ITestCaseService testCaseService, ISchedulerService schedulerService, IMediator mediator)
            : base(logger, testCaseService, schedulerService, mediator)
        {
            DataHelper = new DataHelper();
        }
    }
}