using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ISearchResult<TId>
    {
        bool HasReadyResult { get; }
        IEnumerable<TId> ReadyResult { get; }
        IEnumerable<Task<TId>> AsyncResult { get; }
    }
}
