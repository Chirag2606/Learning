using Cyara.Shared.Types.Authorisation;

namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class UserRoleAccessData
    {
        public string Header { get; set; }

        public string Category { get; set; }

        public string[] Roles { get; set; }

        public RoleAccessToArea[] AccessForRoles { get; set; }

        public class RoleAccessToArea
        {
            public SecuredArea AreaCode { get; set; }

            public string Area { get; set; }

            public string Access { get; set; }

            public RoleAccess[] RoleHasAccess { get; set; }
        }

        public class RoleAccess
        {
            public string Role { get; set; }

            public bool CanAccess { get; set; }
        }
    }
}