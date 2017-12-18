using System;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchResult<TSearchResult>
    {
        Task ForEachAsync(Func<TSearchResult, Task<bool>> action);
    }
}
