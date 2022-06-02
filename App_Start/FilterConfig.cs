namespace Cyara.Web.Portal
{
    using System.Web.Mvc;

    using Cyara.Web.Portal.Core.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AjaxHandleErrorAttribute());

            // this anti forgery needs to go after the Authorize so it doesn't fire when the user is not logged on.
            // 6588 Portal MVC->Portal getting crashed particular scenario after logout
            filters.Add(new ValidateAntiForgeryTokenOnAllPosts { ActionsToSkip = new[] { "Logout", "Bounce", "CspReport" }, ControllersToSkip = new[] { "Sms" } }, int.MaxValue);
        }
    }
}