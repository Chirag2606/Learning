namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;

    public interface IDataHelper
    {
        DateTime Output(DateTime val);

        DateTime? Output(DateTime? val);

        string OutputString(DateTime? val);
    }
}
