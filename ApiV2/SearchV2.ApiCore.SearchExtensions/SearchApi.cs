using SearchV2.Abstractions;
using System.Threading.Tasks;

namespace SearchV2.ApiCore.SearchExtensions
{
    public static class SearchApi
    {
        public static Task<ResponseBody> Find<TSearchQuery, TFilterQuery>(ISearchService<TSearchQuery, TFilterQuery> s, SearchRequest<TSearchQuery, TFilterQuery> r)
            => s.FindAsync(
                r.Query.Search,
                r.Query.Filters,
                (r.PageNumber.Value - 1) * r.PageSize.Value,
                r.PageSize.Value
            );

        public static Task<ResponseBody> Find<TSearchQuery, TFilterQuery>(ISearchService<TSearchQuery, TFilterQuery> s, SearchRequest<TSearchQuery> r)
            => Find(s, new SearchRequest<TSearchQuery, TFilterQuery> { Query = new SearchRequest<TSearchQuery, TFilterQuery>.Body { Filters = default(TFilterQuery), Search = r.Query.Search }, PageNumber = r.PageNumber, PageSize = r.PageSize });
    }
}
