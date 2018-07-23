using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class ReindexingCatalogDb
    {
        [Obsolete("this class ensures no consistency on inserts and deletes")]
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
        
        Task ICatalogDb<string, TFilterQuery, TData>.AddAsync(IEnumerable<TData> items)
            => Task.WhenAll(
                _searchIndex.Add(items.Cast<ISearchIndexItem>()),
                _innerCatalog.AddAsync(items)
            );
        
        Task ICatalogDb<string, TFilterQuery, TData>.DeleteAsync(IEnumerable<string> ids)
            => Task.WhenAll(
                _searchIndex.Remove(ids),
                _innerCatalog.DeleteAsync(ids)
            );

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetAsync(IEnumerable<string> ids) => _innerCatalog.GetAsync(ids);

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<string> ids, TFilterQuery filters) => _innerCatalog.GetFilteredAsync(ids, filters);

        Task<TData> ICatalogDb<string, TFilterQuery, TData>.OneAsync(string id) => _innerCatalog.OneAsync(id);
    }
}
