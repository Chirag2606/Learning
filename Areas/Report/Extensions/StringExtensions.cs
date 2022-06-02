namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class StringExtensions
    {
        public static IEnumerable<int> ParseCsvIntegers(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var items = new List<int>();

            var components = value.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var component in components)
            {
                int parsed;

                if (int.TryParse(component, out parsed))
                {
                    items.Add(parsed);
                }
                else
                {
                    return null;
                }
            }

            return items;
        }
    }
}