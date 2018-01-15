using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchService<TSearchQuery, TFilterQuery>
    {
#warning shoud be refactored to return Task<TData> or something, not just object
        Task<object> FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int skip, int take);
    }
}
