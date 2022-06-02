namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class AuditLogViewData
    {
        public int JournalId { get; set; }

        public string AccountName { get; set; }

        public string DateCreated { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public string Result { get; set; }

        public string Detail { get; set; }

        public string IpAddress { get; set; }

        public string SessionId { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }
    }
}