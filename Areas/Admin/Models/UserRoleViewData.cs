namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class UserRoleViewData
    {
        public bool ReadOnly { get; set; }

        public bool Selected { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Category { get; set; }
    }
}