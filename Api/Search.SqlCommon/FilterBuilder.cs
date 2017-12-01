using Search.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Search.SqlCommon
{
    public static class FilterBuilder
    {
        static IEnumerable<string> BuildFilter(NamedFilter val)
        {
            if (val.Min != null)
            {
                yield return $"{val.Name}>={val.Min}";
            }
            if (val.Max != null)
            {
                yield return $"{val.Name}<={val.Max}";
            }
        }

        public static string BuildFilters(IEnumerable<NamedFilter> filters) => string.Join(" AND ", filters.SelectMany(f => BuildFilter(f)));

        public static NamedFilter ToNamed<T>(this Filter<T> filter, string name) where T : struct
            => new NamedFilter
            {
                Name = name,
                Max = filter.Max.HasValue ? (object)filter.Max.Value : null,
                Min = filter.Min.HasValue ? (object)filter.Min.Value : null
            };
    }
}
