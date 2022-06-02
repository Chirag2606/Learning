namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using Cyara.Shared.Types.Notification;

    public class EmailLogViewData
    {
        public int LogId { get; set; }

        public string Date { get; set; }

        public string Subject { get; set; }

        public SendStatus Status { get; set; }

        public string Result { get; set; }

        public AttachmentItem[] Attachments { get; set; }

        public struct AttachmentItem
        {
            public string FileName { get; set; }

            public string Url { get; set; }
        }
    }
}