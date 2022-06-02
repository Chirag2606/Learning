namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static class ControllerTypeSpecifications
    {
        public static IEnumerable<KeyValuePair<string, Type>> ByAreaName(this IEnumerable<KeyValuePair<string, Type>> query, string areaName)
        {
            var areaNameToFind = string.Format(CultureInfo.InvariantCulture, ".areas.{0}.", areaName);

            return query.Where(x => x.Key.IndexOf(areaNameToFind, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> ByVersion(this IEnumerable<KeyValuePair<string, Type>> query, decimal version)
        {
            var minorVer = (version - Math.Truncate(version)) * 10;
            var versionToFind = (minorVer <= 0)
                                    ? string.Format(CultureInfo.InvariantCulture, ".controllers.v{0}.", Math.Truncate(version))
                                    : string.Format(CultureInfo.InvariantCulture, ".controllers.v{0}_{1}.", Math.Truncate(version), Math.Truncate(minorVer));

            return query.Where(x => x.Key.IndexOf(versionToFind, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> WithoutAreaName(this IEnumerable<KeyValuePair<string, Type>> query)
        {
            return query.Where(x => x.Key.IndexOf(".areas.", StringComparison.OrdinalIgnoreCase) == -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> ByControllerName(this IEnumerable<KeyValuePair<string, Type>> query, string controllerName)
        {
            var controllerNameToFind = string.Format(CultureInfo.InvariantCulture, ".{0}{1}", controllerName, VersionedApiHttpControllerSelector.ControllerSuffix);

            return query.Where(x => x.Key.EndsWith(controllerNameToFind, StringComparison.OrdinalIgnoreCase));
        }
    }
}