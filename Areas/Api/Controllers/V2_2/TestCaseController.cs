// <copyright file="TestCaseController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace
namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_2
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;

    using MediatR;

    public class TestCaseController : V2_1.TestCaseController
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

        /// <summary>
        /// Get the details of a specific test case (id)
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Test Case ID</param>
        [HttpGet, ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.View })]
        public override HttpResponseMessage Get(int account, string scope, int id)
        {
            var session = new ApiSessionFacade(ControllerContext);
            var loadResponse = TestCase.Load(TestCaseService, session.User, account, scope, id);
            return loadResponse.Construct(this, Logger);
        }
    }
}