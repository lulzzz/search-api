using System.Threading.Tasks;

namespace Search.Abstractions.Batching
{
    public interface IBatchedSearchResult<TId>
    {
        Task<TId[]> Next(int batchSize);
    }
}
