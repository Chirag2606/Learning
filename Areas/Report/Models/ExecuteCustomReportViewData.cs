namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Reports;

    [Serializable]
    public class ExecuteCustomReportViewData
    {
        [LoggingIgnore]
        public string PathAndName { get; set; }

      
        public string Name { get; set; }

       
        public string Path { get; set; }

        public int Id { get; set; }

        [LoggingIgnore]
        public int Total { get; set; }

        [LoggingIgnore]
        public int Success { get; set; }

        [LoggingIgnore]
        public int Satisfactory { get; set; }

        /// <summary>
        /// All Failed, Aborted or Internal Error are considered as Failed
        /// </summary>
        [LoggingIgnore]
        public int Failed { get; set; }

        [LoggingIgnore]
        public int SuccessRate { get; set; }

        /// <summary>
        /// can be used as CSS class, etc.
        /// </summary>
        [LoggingIgnore]
        public string ResultStatus { get; set; }

        [LoggingIgnore]
        public CustomSeverityLevel Severity { get; set; }

        /// <summary>
        /// can be used as a readable label
        /// </summary>
        [LoggingIgnore]
        public string SeverityLabel { get; set; }

        /// <summary>
        /// can be used as CSS class
        /// </summary>
        [LoggingIgnore]
        public string SeverityStatus { get; set; }
    }
}