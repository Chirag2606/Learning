namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;

    public static class SelectListItemExtensions
    {
        public static IEnumerable<SelectListItem> SetSelected(this IEnumerable<SelectListItem> value, string current)
        {
            value.ForEach(x => x.Selected = false);

            var match = value.SingleOrDefault(x => x.Value == current);
            if (match != null)
            {
                match.Selected = true;
            }

            return value;
        }
    }
}
