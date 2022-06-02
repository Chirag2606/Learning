namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Web.Portal.Models;

    public static class SupportedTags
    {
        public static string CreateJson()
        {
            var listOfTags = new[]
                                 {
                                     new Tag { Code = "{FirstName}", Factory = TagFactory.BasicTag },
                                     new Tag { Code = "{LastName}", Factory = TagFactory.BasicTag },
                                     new Tag { Code = "{Username}", Factory = TagFactory.BasicTag }
                                 };

            return listOfTags.ToJson(true);
        }
    }
}
