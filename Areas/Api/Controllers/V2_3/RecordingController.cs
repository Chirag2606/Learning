// <copyright file="RecordingController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace
namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_3
{
    using Cyara.Foundation.Core.IO;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Web.Types.TestCase;

    public class RecordingController : V2_2.RecordingController
    {
        public RecordingController(ILogger logger, ITestCaseService testCaseService, IFileSystem fileSystem, IMediaAccess mediaAccess)
            : base(logger, testCaseService, fileSystem, mediaAccess)
        {
        }
    }
}