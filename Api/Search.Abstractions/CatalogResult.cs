using System.Collections.Generic;

namespace Search.Abstractions
{
    public class CatalogResult<TData>
    {
        public IEnumerable<TData> Data { get; set; }
        public int? Count { get; set; }
    }
}
