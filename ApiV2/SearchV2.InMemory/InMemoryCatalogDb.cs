using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.InMemory
{
    public class InMemoryCatalogDb<TId, TFilterQuery, TData> : ICatalogDb<TId, TFilterQuery, TData> where TData : class, IWithReference<TId>
    {
        public delegate Func<TData, bool> FilterCreatorDelegate(TFilterQuery filters);

        readonly Dictionary<TId, TData> _dictionary;
        readonly FilterCreatorDelegate _filterToPredicate;

        public InMemoryCatalogDb(IEnumerable<TData> data, FilterCreatorDelegate filterCreator)
        {
            _dictionary = data.ToDictionary(d => d.Ref);
            _filterToPredicate = filterCreator;
        }

        Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetAsync(IEnumerable<TId> ids)
        => Task.FromResult(ids.Select(id => _dictionary.TryGetValue(id, out TData data) ? data : null));

        Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters)
        {
            var filterFunc = _filterToPredicate(filters);
            return Task.FromResult(
                ids
                    .Select(id => _dictionary.TryGetValue(id, out TData data) ? data : null)
                    .Where(d => d != null && filterFunc(d))
                );
        }

        Task<TData> ICatalogDb<TId, TFilterQuery, TData>.OneAsync(TId id)
        => Task.FromResult(_dictionary.TryGetValue(id, out TData data) ? data : null);
    }
}
