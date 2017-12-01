using System;
using System.Collections.Generic;

namespace Search.Abstractions
{
    public interface ISearchResult<TId> : IEnumerable<TId>, IDisposable { }
}
