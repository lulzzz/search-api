using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class ReindexingCatalogDb
    {
        public static ICatalogDb<string, TFilterQuery, TData> NotifyIndexOnChanges<TFilterQuery, TData>(this ICatalogDb<string, TFilterQuery, TData> catalog, ISearchIndex searchIndex) where TData : ISearchIndexItem
        {
            return new ReindexingCatalogDb<TFilterQuery, TData>(catalog, searchIndex);
        }
    }

    class ReindexingCatalogDb<TFilterQuery, TData> : ICatalogDb<string, TFilterQuery, TData> where TData : ISearchIndexItem
    {
        readonly ICatalogDb<string, TFilterQuery, TData> _innerCatalog;
        readonly ISearchIndex _searchIndex;

        internal ReindexingCatalogDb(ICatalogDb<string, TFilterQuery, TData> innerCatalog, ISearchIndex searchIndex)
        {
            _innerCatalog = innerCatalog;
            _searchIndex = searchIndex;
        }

        async Task ICatalogDb<string, TFilterQuery, TData>.AddAsync(IEnumerable<TData> items)
        {
            await _searchIndex.Add(items.Cast<ISearchIndexItem>());
            await _innerCatalog.AddAsync(items);
        }

        async Task ICatalogDb<string, TFilterQuery, TData>.DeleteAsync(IEnumerable<string> ids)
        {
            await _searchIndex.Remove(ids);
            await _innerCatalog.DeleteAsync(ids);
        }

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetAsync(IEnumerable<string> ids) => _innerCatalog.GetAsync(ids);

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<string> ids, TFilterQuery filters) => _innerCatalog.GetFilteredAsync(ids, filters);

        Task<TData> ICatalogDb<string, TFilterQuery, TData>.OneAsync(string id) => _innerCatalog.OneAsync(id);
    }
}
