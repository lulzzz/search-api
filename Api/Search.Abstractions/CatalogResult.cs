using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Abstractions
{
    public class CatalogResult<TData>
    {
        public IEnumerable<TData> Data { get; set; }
        public int? Count { get; set; }
    }
}
