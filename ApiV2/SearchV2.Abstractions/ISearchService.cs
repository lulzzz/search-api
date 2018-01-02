using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchService<TSearchQuery, TFilterQuery>
    {
        Task<object> FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int skip, int take);
    }
}
