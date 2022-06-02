// <copyright file="RecordingController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace
namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_2
{
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.IO;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [AuthorizeAccount(StaticRoles.Admin, StaticRoles.Reporting)]
    [HandleApiException]
    public class RecordingController : V2_1.RecordingController
    {
        public RecordingController(ILogger logger, ITestCaseService testCaseService, IFileSystem fileSystem, IMediaAccess mediaAccess)
            : base(logger, testCaseService, fileSystem, mediaAccess)
        {
        }
    }
}
