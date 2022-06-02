namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;

    public class UserListViewData
    {
        public bool Active { get; set; }
        
        public string Firstname { get; set; }

        public bool IsLockedOut { get; set; }

        public string Lastname { get; set; }
        
        public string Telephone { get; set; }

        public Guid UserId { get; set; }

        public string Username { get; set; }
    }
}