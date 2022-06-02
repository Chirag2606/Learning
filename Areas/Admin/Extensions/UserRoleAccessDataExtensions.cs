namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Portal.Areas.Admin.Models;

    using static Cyara.Web.Portal.Areas.Admin.Models.UserRoleAccessData;

    public static class UserRoleAccessDataExtensions
    {
        // sorting/grouping and formatting for better output on UI, replace enums with their resource strings
        public static IList<SecuredArea> SecuredAreaDisplayOrder => new[]
            {
                SecuredArea.CxModel,
                SecuredArea.TestCase,
                SecuredArea.Block,
                SecuredArea.Agent,
                SecuredArea.Campaign,
                SecuredArea.Report,
                SecuredArea.Dashboard,
                SecuredArea.AgentReport,
                SecuredArea.TestTools,
                SecuredArea.Integration,
                SecuredArea.Account,
                SecuredArea.AccountReport,
                SecuredArea.AccountUser,
                SecuredArea.LoadTestCalendar
            };

        public static UserRoleAccessData Prime(this UserRoleAccessData value, string category)
        {
            // Check we have a valid category with roles
            var roles = StaticCategories.GetRolesForCategory(category);
            if (roles == null || roles.Length <= 0)
            {
                return value;
            }

            switch (category)
            {
                case StaticCategories.QA:
                    value.Header = Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.QARoleCategoryDescription;
                    break;
                case StaticCategories.Business:
                    value.Header = Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.BusinessRoleCategoryDescription;
                    break;
                case StaticCategories.Operations:
                    value.Header = Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.OperationsRoleCategoryDescription;
                    break;
                case StaticCategories.Development:
                    value.Header = Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.DevelopmentRoleCategoryDescription;
                    break;
                case StaticCategories.Administration:
                    value.Header = Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.AdministrationRoleCategoryDescription;
                    break;
            }

            var roleAccessListToArea = new List<RoleAccessToArea>();
            var mapSecuredArea = EnumLabelHelper.SecuredArea.AsDictionary();
            var mapSecureAccess = EnumLabelHelper.SecureAccess.AsDictionary();

            var roleSet = ClaimsIdentityExtensions.GetRoleAccess();
            var roleList = roles.Select(x => new { Role = x, AreaAccess = roleSet.AccessFor(x) }).ToList();

            // Construct the matrix of role area access
            foreach (var area in mapSecuredArea.Keys)
            {
                var validAccess = area.ValidAccess();
                var roleAccessListForArea = mapSecureAccess.Keys
                    .Where(x => validAccess.Has(x))
                    .Select(access => new RoleAccessToArea
                    {
                        AreaCode = area,
                        Access = mapSecureAccess[access],
                        RoleHasAccess = roleList
                           .Select(r => new RoleAccess
                           {
                               Role = r.Role,
                               CanAccess = r.AreaAccess.AccessFor(area).Has(access)
                           }).ToArray()
                    }).ToList();

                if (roleAccessListForArea.Count > 0)
                {
                    roleAccessListToArea.AddRange(roleAccessListForArea);
                }
            }

            // no access - just return
            if (!roleAccessListToArea.Any())
            {
                return value;
            }
            
            roleAccessListToArea = OrderRoleAccessToArea(roleAccessListToArea, mapSecuredArea);

            // Update the value
            value.Category = category;

            // Replace roles with the display name
            var mapRoles = MapRolesToResources(roles);
            value.Roles = roles.Select(x => mapRoles.ContainsKey(x) ? mapRoles[x] : x).ToArray();
            value.AccessForRoles = roleAccessListToArea.ToArray();

            return value;
        }

        public static UserRoleAccessData Prime(
            this UserRoleAccessData value,
            IList<UserRoleViewData> legacyAccountRoles,
            IList<UserRoleViewData> accountRoles,
            IList<UserRoleViewData> platformRoles,
            string userName)
        {
            var roleAccessListToArea = new List<RoleAccessToArea>();
            
            var roles = new List<string>();
            roles.AddRange(legacyAccountRoles.Where(x => x.Selected).Select(x => x.Value));
            roles.AddRange(accountRoles.Where(x => x.Selected).Select(x => x.Value));
            roles.AddRange(platformRoles.Where(x => x.Selected).Select(x => x.Value));
            var list = ClaimsIdentityExtensions.GetRoleAccess();

            var mapSecuredArea = EnumLabelHelper.SecuredArea.AsDictionary();
            var mapSecureAccess = EnumLabelHelper.SecureAccess.AsDictionary();

            // Construct the matrix of role area access
            foreach (var area in mapSecuredArea.Keys)
            {
                var validAccess = area.ValidAccess();
                var roleAccessListForArea = mapSecureAccess.Keys
                    .Where(x => validAccess.Has(x))
                    .Select(access => new RoleAccessToArea
                    {
                        AreaCode = area,
                        Access = mapSecureAccess[access],
                        RoleHasAccess = new[]
                           {
                               new RoleAccess
                                    {
                                        CanAccess = roles.Any(x => list.AccessFor(x).AccessFor(area).Has(access))
                                    }
                           }
                    }).ToList();

                if (roleAccessListForArea.Count > 0)
                {
                    roleAccessListToArea.AddRange(roleAccessListForArea);
                }
            }

            // no access - just return
            if (!roleAccessListToArea.Any())
            {
                return value;
            }
            
            roleAccessListToArea = OrderRoleAccessToArea(roleAccessListToArea, mapSecuredArea);

            value.AccessForRoles = roleAccessListToArea.ToArray();

            var header = string.Format(Cyara.Web.Resources.Areas.Admin.Views.Shared.EditorTemplates.UserEditViewModel_cshtml.ViewPrivilegesText, userName);
            value.Header = header;

            return value;
        }

        private static List<RoleAccessToArea> OrderRoleAccessToArea(List<RoleAccessToArea> roleAccessListToArea, IDictionary<SecuredArea, string> mapSecuredArea)
        {
            roleAccessListToArea = roleAccessListToArea.OrderBy(x => SecuredAreaDisplayOrder.IndexOf(x.AreaCode)).ThenBy(c => c.Access).ToList();

            // Set the display for each area, and remove the duplicated ones to simplify display logic
            var areaCode = SecuredArea.None;
            foreach (var item in roleAccessListToArea)
            {
                if (item.AreaCode.Equals(areaCode))
                {
                    item.Area = string.Empty;
                }
                else
                {
                    areaCode = item.AreaCode;
                    item.Area = mapSecuredArea[item.AreaCode];
                }
            }

            return roleAccessListToArea;
        }

        private static Dictionary<string, string> MapRolesToResources(string[] roles)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (roles == null || roles.Length <= 0)
            {
                return dict;
            }

            foreach (var role in roles)
            {
                var description = MapRoleToResource(role);
                if (!string.IsNullOrEmpty(description))
                {
                    dict.Add(role, description);
                }
            }

            return dict;
        }

        public static string MapRoleToResource(string roleCode)
        {
            return Shared.Resources.Resource.GetResourceString("/Common", $"Role_{roleCode}", null, "Cyara.Web.Api.Resources") ?? roleCode;
        }
    }
}