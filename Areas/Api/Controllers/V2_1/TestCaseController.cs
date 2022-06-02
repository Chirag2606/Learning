namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models.V2;

    using MediatR;

    public class TestCaseController : V2.TestCaseController
    {
        public TestCaseController(
            ILogger logger,
            ITestCaseService testCaseService,
            ISchedulerService schedulerService,
            IMediator mediator)
            : base(logger, testCaseService, schedulerService, mediator)
        {
        }

        [AuthorizeArea(testCase: new[] { Access.View })]
        public override async Task<HttpResponseMessage> TestGet(int account, string scope, int id, Guid? entity = null)
        {
            return await InternalTestGet<TestCaseResult2, TestCaseResult, TestStepResultList>(account, scope, id, false, entity);
        }
    }
}
