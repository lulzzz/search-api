using System.Threading.Tasks;

namespace SearchV2.ApiCore
{
    public interface ISearchStrategy<TSearchQuery, TFilterQuery>
    {
        Task<object> FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int skip, int take);
    }
}
