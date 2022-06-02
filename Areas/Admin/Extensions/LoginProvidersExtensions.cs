namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using Cyara.Shared.Types.Authorisation;

    public static class LoginProvidersExtensions
    {
        public static string ToLabel(this LoginProviders value)
        {
            return value.ToString().ToUpper();
        }

        public static string UsernameTooltip(this LoginProviders value)
        {
            if (value.HasFlag(LoginProviders.Google))
            {
                return "UsernameAsGoogleEmail";
            }

            return "UsernameUnique";
        }
    }
}