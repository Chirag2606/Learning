namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.Collections.Generic;

    public class EmailLogResultViewData
    {
        public EmailLogResultViewData()
        {
            LogResults = new List<EmailLogDetail>();
        }

        public List<EmailLogDetail> LogResults { get; set; }

        public EmailLogResultViewData LogFailure(string reason, string[] emailAddresses, string detail)
        {
            LogResults.Add(new EmailLogDetail { Reason = reason, EmailAddresses = emailAddresses, Detail = detail });

            return this;
        }

        public EmailLogResultViewData LogSending(string reason)
        {
            LogResults.Add(new EmailLogDetail { Reason = reason });

            return this;
        }

        public EmailLogResultViewData LogSucceeded(string reason)
        {
            LogResults.Add(new EmailLogDetail { Reason = reason });

            return this;
        }

        public EmailLogResultViewData LogFailed(string reason)
        {
            LogResults.Add(new EmailLogDetail { Reason = reason });

            return this;
        }

        public EmailLogResultViewData LogRaw(string reason)
        {
            LogResults.Add(new EmailLogDetail { Reason = reason });

            return this;
        }

        public class EmailLogDetail
        {
            public string Reason { get; set; }
            
            public string[] EmailAddresses { get; set; }
            
            public string Detail { get; set; }
        }
    }
}