namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    [Serializable]
    public class NextRunViewData
    {
        public string NextRun { get; set; }

        public bool IsActive { get; set; }
    }
}