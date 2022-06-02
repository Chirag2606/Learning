namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Web.Portal.Areas.Api.Core;

    public class DataHelper : IDataHelper
    {
        public DateTime? Output(DateTime? val)
        {
            return val.SetUtcKind();
        }

        public DateTime Output(DateTime val)
        {
            return val.SetUtcKind();
        }

        public string OutputString(DateTime? val)
        {
            if (val == null)
            {
                return null;
            }

            return val.Value.SetUtcKind().ToString("HH:mm:ssZ");
        }
    }
}
