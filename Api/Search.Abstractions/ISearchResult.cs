using System;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ISearchResult<TId>
    {
        //        bool HasReadyResult { get; }
        //Task<IEnumerable<TId>> ReadyResult { get; }
        //#warning extremely unsafe - enumeration can yield more tasks then actual results count. should be replaced with some kind of IAsyncEnumerable
        //        IEnumerable<Task<TId>> AsyncResult { get; }

        Task ForEach(Func<TId, Task<bool>> body);
        //Task<IEnumerable<TResult>> ForEach<TResult>(Func<TId, Task<(TResult, bool)>> body);
    }
}
