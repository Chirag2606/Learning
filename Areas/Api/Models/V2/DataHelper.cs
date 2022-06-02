namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;

    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Areas.Api.Extensions;

    public class DataHelper : IDataHelper
    {
        public DateTime? Output(DateTime? val)
        {
            return val.SetLocalKind();
        }

        public DateTime Output(DateTime val)
        {
            return val.SetLocalKind();
        }

        public string OutputString(DateTime? val)
        {
            if (val == null)
            {
                return null;
            }

            return val.Value.SetLocalKind().ToString("HH:mm:ss.FFFFFFzzz");
        }
    }
}